using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CodeCapital.AspNetCore.Extensions.Configuration.Json
{
    public class RemoteJsonFileService
    {
        private readonly HttpClient _client;
        private readonly ILogger<RemoteJsonFileService> _logger;

        public RemoteJsonFileService(HttpClient client, ILogger<RemoteJsonFileService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Stream?> Get(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode) return await response.Content.ReadAsStreamAsync();

                _logger.LogError($@"RemoteJsonConfigurationProvider, url {_client.BaseAddress.AbsoluteUri}, {response.StatusCode}, error: {response.Content}, {response.ReasonPhrase}");

                return null;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($@"RemoteJsonConfigurationProvider, {_client.BaseAddress.AbsolutePath}, error: {e.Message}");
            }

            return null;
        }
    }
}
