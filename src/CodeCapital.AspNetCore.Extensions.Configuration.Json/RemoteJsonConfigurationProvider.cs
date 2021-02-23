using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CodeCapital.AspNetCore.Extensions.Configuration.Json
{
    public class RemoteJsonConfigurationProvider : ConfigurationProvider
    {
        private readonly string _url;
        private readonly RemoteJsonFileService _remoteJsonFileService;
        private readonly ILogger _logger;

        public RemoteJsonConfigurationProvider(string url, RemoteJsonFileService remoteJsonFileService, ILoggerFactory loggerFactory)
        {
            _url = url;
            _remoteJsonFileService = remoteJsonFileService;
            _logger = loggerFactory.CreateLogger<RemoteJsonConfigurationProvider>();
        }

        public override void Load() => LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        private async Task LoadAsync()
        {
            var stream = await GetStreamAsync();

            if (stream == null)
            {
                _logger.LogWarning("Empty stream. Remote json file wasn't added to the configuration.");
                return;
            }

            try
            {
                Data = JsonConfigurationFileParser.Parse(stream);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Problem to parse remote json file.");
            }
        }

        private Task<Stream?> GetStreamAsync() => _remoteJsonFileService.Get(_url);
    }
}
