using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCapital.Services.MailGun
{
    public static class MailGunServiceCollectionExtensions
    {
        // Maybe just used the 2nd option
        //public static void AddMailGun(this IServiceCollection services, MailGunSettings configureSettings)
        //{
        //    services.TryAddSingleton(Options.Create(configureSettings));
        //    services.AddHttpClient<IEmailSender, MailGunEmailSender>();
        //    //services.TryAddSingleton<IEmailSender, MailGunService>();
        //}

        public static void AddMailGun(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailGunSettings>(options => configuration.GetSection(nameof(MailGunSettings)).Bind(options));
            services.AddHttpClient<IEmailSender, MailGunEmailSender>();
        }

        // Maybe just used the 2nd option
        //public static void AddMailGun(this IServiceCollection services, Action<MailGunSettings> configuration)
        //{
        //    var configureOptions = new MailGunSettings();

        //    configuration(configureOptions);

        //    AddMailGun(services, configureOptions);
        //}
    }
}
