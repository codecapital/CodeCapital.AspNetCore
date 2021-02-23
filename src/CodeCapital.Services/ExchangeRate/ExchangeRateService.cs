using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodeCapital.Services.ExchangeRate
{
    public class ExchangeRateService
    {
        private readonly ILogger<ExchangeRateService> _logger;
        public HttpClient Client { get; }
        public const string Url = "https://api.exchangeratesapi.io";

        public ExchangeRateService(HttpClient client, ILogger<ExchangeRateService> logger)
        {
            _logger = logger;
            Client = client;
        }

        /// <summary>
        /// Get Exchange Rate with a base e.g GBP and response in EUR, USD
        /// </summary>
        /// <param name="baseRate">any code e.g. GBP</param>
        /// <param name="symbols">any currency codes</param>
        /// <returns></returns>
        public async Task<RateResponse> GetRateAsync(string baseRate, string[] symbols)
        {
            try
            {
                var response = await Client.GetStreamAsync($"{Url}/latest?base={baseRate}&symbols={string.Join(",", symbols)}");

                var rate = await JsonSerializer.DeserializeAsync<RateResponse>(response,
                    new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        IgnoreNullValues = true,
                        PropertyNameCaseInsensitive = true
                    });

                return rate ?? new RateResponse();
            }
            catch (Exception e)
            {
                _logger.LogError("Request Failed", e);
            }

            return new RateResponse();
        }
    }
}
