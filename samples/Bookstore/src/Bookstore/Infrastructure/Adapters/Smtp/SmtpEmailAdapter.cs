using Bookstore.API.Options;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using Bookstore.Domain.Model.Ports;

namespace Bookstore.Infrastructure.Adapters.Smtp
{
    public class SmtpEmailAdapter : IEmailPort
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailAdapter(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.EnableSsl, ct);
        
            if (!string.IsNullOrEmpty(_smtpSettings.Username))
            {
                await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, ct);
            }

            await smtp.SendAsync(email, ct);
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
