using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CodeCapital.Services.Captcha
{
    public class CaptchaService
    {
        private readonly ILogger<CaptchaService> _logger;
        private readonly HttpClient _client;
        private readonly ReCaptchaSettings _settings;
        private const string Url = "https://www.google.com/recaptcha/api/siteverify?secret=";
        public string CaptchaFormName => "g-recaptcha-response";
        public ReCaptchaSettings Settings => _settings;

        public CaptchaService(HttpClient client, IOptions<ReCaptchaSettings> settings, ILogger<CaptchaService> logger)
        {
            _logger = logger;
            _client = client;
            _settings = settings.Value;
        }

        /// <summary>
        /// Validates captcha response
        /// </summary>
        /// <param name="clientResponse">This represents form field g-recaptcha-response</param>
        /// <returns></returns>
        public async Task<CaptchaResponseDto> ValidateAsync(string clientResponse)
        {
            try
            {
                var response = await _client.GetFromJsonAsync<CaptchaResponseDto>($"{Url}{_settings.SecretKey}&response={clientResponse}");

                return Process(response);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "CaptchaService");

                return FailedCaptchaResponseDto(e.Message);
            }

            static CaptchaResponseDto Process(CaptchaResponseDto? dto)
            {
                if (dto == null) return FailedCaptchaResponseDto("Captcha wasn't parsed");

                var error = dto.ErrorCodes == null || dto.ErrorCodes.Count == 0 ? "" : dto.ErrorCodes[0].ToLower();

                dto.Message = error switch
                {
                    "missing-input-secret" => "The secret parameter is missing.",
                    "invalid-input-secret" => "The secret parameter is invalid or malformed.",
                    "missing-input-response" => "The response parameter is missing.",
                    "invalid-input-response" => "The response parameter is invalid or malformed.",
                    "bad-request" => "The request is invalid or malformed.",
                    "timeout-or-duplicate" => "The response is no longer valid: either is too old or has been used previously.",
                    _ => "Error occurred. Please try again",
                };

                return dto;
            }

            static CaptchaResponseDto FailedCaptchaResponseDto(string message)
                => new() { Success = false, Message = message };
        }

        //public ReCaptchaSettings GetSettings() => _settings;
    }
}
