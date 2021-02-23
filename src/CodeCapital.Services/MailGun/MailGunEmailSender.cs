using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CodeCapital.Services.MailGun
{
    /// <summary>
    /// MailGun Service to send emails using MailGun API and implementing IEmailSender, registered as services.AddHttpClient() through IHttpClientFactory 
    /// </summary>
    public class MailGunEmailSender : IEmailSender
    {
        private readonly ILogger<MailGunEmailSender> _logger;
        private readonly MailGunSettings _settings;
        private readonly HttpClient _client;

        public MailGunEmailSender(HttpClient client, IOptions<MailGunSettings> options, ILogger<MailGunEmailSender> logger)
        {
            _logger = logger;
            _settings = options.Value;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{_settings.ApiKey}")));

            CheckInitialisation();

            _client = client;
        }


        private void CheckInitialisation()
        {
            if (string.IsNullOrEmpty(_settings.ApiUrl) || string.IsNullOrEmpty(_settings.ApiKey))
                _logger.LogError("Missing ApiUrl or ApiKey in your MailGunSettings");
        }

        public async Task<HttpResponseMessage?> SendEmailAsync(string to, string from, string fromName, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException(nameof(to));

            if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException(nameof(from));

            return await SendAsync(ComposeEmail(to, from, fromName, subject, htmlMessage));
        }


        /// <summary>
        /// Default email sending from ASP.NET Core Interface
        /// </summary>
        /// <param name="email">Recipient to</param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(nameof(email));

            var response = await SendAsync(ComposeEmail(email, _settings.DefaultFrom, _settings.DefaultFromName, subject, htmlMessage));
        }

        /// <summary>
        /// System message sent to a recipient from the MailgunSettings
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage?> SendEmailAsync(string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_settings.DefaultTo)) throw new ArgumentException(nameof(_settings.DefaultTo));

            return await SendAsync(ComposeEmail(_settings.DefaultTo, _settings.DefaultFrom, _settings.DefaultFromName, subject, htmlMessage));
        }

        public async Task<HttpResponseMessage?> SendAsync(MultipartFormDataContent dataContent)
        {
            try
            {
                var response = await _client.PostAsync($"{_settings.ApiUrl}/messages.mime", dataContent);

                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Error when trying to send mail, response: {@response}", response);
                else _logger.LogInformation("Email sent successfully");

                return response;

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Sending email");
            }

            return null;
        }

        private async Task<HttpResponseMessage?> SendAsync(Dictionary<string, string> emailDictionary)
        {
            try
            {
                var response = await _client.PostAsync($"{_settings.ApiUrl}/messages", new FormUrlEncodedContent(emailDictionary));

                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Error when trying to send mail, response: {@response}", response);
                else _logger.LogInformation("Email sent successfully");

                return response;

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Sending email");
            }

            return null;
        }

        private static Dictionary<string, string> ComposeEmail(string to, string from, string fromName, string subject, string body) => new Dictionary<string, string>
        {
            ["from"] = $"{fromName ?? ""}<{from}>",
            ["to"] = to,
            ["subject"] = subject,
            ["html"] = body
        };
    }
}
