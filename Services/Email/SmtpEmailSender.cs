using System.Net;
using System.Net.Mail;
using MedicalOfficeManagement.Models.Email;
using Microsoft.Extensions.Options;

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
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email is required.");

            if (string.IsNullOrWhiteSpace(_options.Host))
                throw new InvalidOperationException("SMTP Host is not configured. Please check appsettings.json");

            var message = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.Username : _options.FromEmail,
                    string.IsNullOrWhiteSpace(_options.FromName) ? "Medical Office" : _options.FromName
                ),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(_options.Host, _options.Port > 0 ? _options.Port : 587)
            {
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                EnableSsl = _options.Port == 587 || _options.Port == 465
            };

            await client.SendMailAsync(message);
        }
    }
}
