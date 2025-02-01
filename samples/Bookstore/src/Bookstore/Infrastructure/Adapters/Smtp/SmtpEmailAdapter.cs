using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using Bookstore.Domain.Model.Ports;
using Bookstore.Infrastructure.Adapters.Smtp.Options;

namespace Bookstore.Infrastructure.Adapters.Smtp
{
    public class SmtpEmailAdapter : IEmailPort
    {
        private readonly SmtpOptions _smtpOptions;

        public SmtpEmailAdapter(IOptions<SmtpOptions> smtpSettings)
        {
            _smtpOptions = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromEmail));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, _smtpOptions.EnableSsl, ct);
        
            if (!string.IsNullOrEmpty(_smtpOptions.Username))
            {
                await smtp.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password, ct);
            }

            await smtp.SendAsync(email, ct);
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
