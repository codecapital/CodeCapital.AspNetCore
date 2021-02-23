using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CodeCapital.AspNetCore.Extensions.Configuration.Json
{
    public static class RemoteJsonConfigurationExtensions
    {
        public static IConfigurationBuilder AddRemoteJsonFile(
            this IConfigurationBuilder builder,
            string url,
            RemoteJsonFileService remoteJsonFileService,
            ILoggerFactory loggerFactory,
            bool reloadOnChange) =>
            builder.Add(new RemoteJsonConfigurationSource(url, remoteJsonFileService, loggerFactory));
    }
}
