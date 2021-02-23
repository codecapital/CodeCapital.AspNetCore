using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Linq;

namespace CodeCapital.Extensions.Configuration.Json
{
    public static class ServiceExtensions
    {
        private static readonly TimeSpan DelayBetweenAttempts = TimeSpan.FromMilliseconds(600);
        private const int RetryCount = 3;

        /// <summary>
        /// Adds AddHttpClient&lt;RemoteJsonFileService&gt; and rebuilds Configuration. It shouldn't throw an exception but logs
        /// errors and warnings.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="keyToUrl"></param>
        /// <param name="options">Returns updated IConfiguration</param>
        /// <param name="reloadOnChange"></param>
        public static void AddRemoteJsonFile(this IServiceCollection services, IConfiguration configuration, string keyToUrl, Action<IConfiguration> options, bool reloadOnChange = false)
        {
            var url = configuration.GetValue<string>(keyToUrl);

            if (string.IsNullOrWhiteSpace(url))
            {
                //Remote url not configured in the key you provided, not sure if we should throw an error   
                options.Invoke(configuration);
                return;
            }

            if (services.All(x => x.ServiceType != typeof(RemoteJsonFileService)))
            {
                services.AddHttpClient<RemoteJsonFileService>().AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(RetryCount, _ => DelayBetweenAttempts));
            }

            var buildServices = services.BuildServiceProvider();

            var loggerFactory = buildServices.GetService<ILoggerFactory>();

            var remoteJsonFileService = buildServices.GetService<RemoteJsonFileService>();

            //var config = new ConfigurationBuilder()
            //    .AddConfiguration(configuration)
            //    .AddRemoteJsonFile(loggerFactory, remoteJsonFileService, reloadOnChange);

            //configuration = config.Build();

            //var config = new ConfigurationBuilder();

            var providers = ((IConfigurationRoot)configuration).Providers.Append(new RemoteJsonConfigurationProvider(url, remoteJsonFileService, loggerFactory)).ToList();

            // add directly to configuration
            configuration = (IConfiguration)(new ConfigurationRoot(providers));

            services.Replace(ServiceDescriptor.Singleton(configuration));

            options.Invoke(configuration);

            //services.Replace(ServiceDescriptor.Singleton(configuration));

            //options.Invoke(configuration);
        }
    }
}
