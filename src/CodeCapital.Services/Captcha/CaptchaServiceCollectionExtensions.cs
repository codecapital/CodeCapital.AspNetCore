using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCapital.Services.Captcha
{
    public static class CaptchaServiceCollectionExtensions
    {
        public static void AddReCaptcha(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ReCaptchaSettings>(options => configuration.GetSection(nameof(ReCaptchaSettings)).Bind(options));
            services.AddHttpClient<CaptchaService>();
        }
    }
}
