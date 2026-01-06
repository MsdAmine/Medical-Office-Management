using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MedicalOfficeManagement.Models.Email;

namespace MedicalOfficeManagement.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                Credentials = new NetworkCredential(
                    _options.Username,
                    _options.Password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}
