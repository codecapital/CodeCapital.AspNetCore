using Microsoft.Extensions.Configuration;
using System.Linq;

namespace CodeCapital.Services
{
    // https://docs.microsoft.com/en-us/aspnet/core/security/ip-safelist?view=aspnetcore-2.2
    /// <summary>
    /// Checks AdminSafeList from configuration
    /// </summary>
    public class AdminSafeListService
    {
        private readonly string[] _safeIpsList;
        private const string AdminSafeList = "AdminSafeList";

        public AdminSafeListService(IConfiguration configuration)
        {
            _safeIpsList = configuration[AdminSafeList].Split(';');
        }

        /// <summary>
        /// Get all whitelisted IP Addresses
        /// </summary>
        /// <returns></returns>
        public string[] Get() => _safeIpsList;

        public bool Check(string ipAddress) => _safeIpsList.Contains(ipAddress);
    }
}
