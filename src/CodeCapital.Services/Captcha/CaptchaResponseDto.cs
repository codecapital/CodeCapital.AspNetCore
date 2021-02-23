using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CodeCapital.Services.Captcha
{
    public class CaptchaResponseDto
    {
        //Must be lowercase
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error-codes")]
        public List<string>? ErrorCodes { get; set; }

        [JsonIgnore]
        public string Message { get; set; } = "";
    }
}