using System;

namespace CodeCapital.Services.MailGun
{
    public class MailGunSettings
    {
        public string ApiKey { get; set; } = default!;
        public string ApiUrl { get; set; } = "";

        [Obsolete("Used instead ApiUrl", true)]
        public string Domain { get; set; } = default!;

        /// <summary>
        /// Default Email
        /// </summary>
        public string DefaultFrom { get; set; } = default!;
        public string DefaultFromName { get; set; } = default!;
        public string? DefaultTo { get; set; }
    }
}
