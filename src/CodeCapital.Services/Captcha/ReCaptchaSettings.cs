namespace CodeCapital.Services.Captcha
{
    public class ReCaptchaSettings
    {
        public string SiteKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;

        /// <summary>
        /// Theme light or dark
        /// </summary>
        public string Theme { get; set; } = "light";
    }
}
