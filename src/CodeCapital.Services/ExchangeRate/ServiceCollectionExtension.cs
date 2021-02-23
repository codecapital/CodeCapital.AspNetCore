using Microsoft.Extensions.DependencyInjection;

namespace CodeCapital.Services.ExchangeRate
{
    public static class ServiceCollectionExtension
    {
        public static void AddExchangeRate(this IServiceCollection services)
        {
            services.AddHttpClient<ExchangeRateService>();
        }
    }
}
