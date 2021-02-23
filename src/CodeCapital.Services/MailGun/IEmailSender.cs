using System.Net.Http;
using System.Threading.Tasks;

namespace CodeCapital.Services.MailGun
{
    public interface IEmailSender
    {
        /// <summary>
        /// Sending email, using default MailGun Settings.
        /// </summary>
        /// <param name="email">Recipient</param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns></returns>
        Task SendEmailAsync(string email, string subject, string htmlMessage);

        Task<HttpResponseMessage?> SendEmailAsync(string subject, string htmlMessage);

        Task<HttpResponseMessage?> SendEmailAsync(string to, string from, string fromName, string subject, string htmlMessage);

        Task<HttpResponseMessage?> SendAsync(MultipartFormDataContent dataContent);
    }
}
