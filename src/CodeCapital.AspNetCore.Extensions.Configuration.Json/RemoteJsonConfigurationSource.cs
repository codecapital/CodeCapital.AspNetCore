using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CodeCapital.AspNetCore.Extensions.Configuration.Json
{
    public class RemoteJsonConfigurationSource : IConfigurationSource
    {
        private readonly string _url;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RemoteJsonFileService _jsonFileService;

        public RemoteJsonConfigurationSource(string url, RemoteJsonFileService jsonFileService, ILoggerFactory loggerFactory)
        {
            _url = url;
            _loggerFactory = loggerFactory;
            _jsonFileService = jsonFileService;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new RemoteJsonConfigurationProvider(_url, _jsonFileService, _loggerFactory);
    }
}
