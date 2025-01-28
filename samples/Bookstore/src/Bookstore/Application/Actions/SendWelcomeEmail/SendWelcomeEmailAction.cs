using OpenDDD.Application;
using Bookstore.Domain.Model.Ports;

namespace Bookstore.Application.Actions.SendWelcomeEmail
{
    public class SendWelcomeEmailAction : IAction<SendWelcomeEmailCommand, object>
    {
        private readonly IEmailSender _emailSender;

        public SendWelcomeEmailAction(IEmailSender emailSender)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        }

        public async Task<object> ExecuteAsync(SendWelcomeEmailCommand command, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(command.RecipientEmail))
                throw new ArgumentException("Recipient email cannot be empty.", nameof(command.RecipientEmail));

            if (string.IsNullOrWhiteSpace(command.RecipientName))
                throw new ArgumentException("Recipient name cannot be empty.", nameof(command.RecipientName));

            var subject = "Welcome to Bookstore!";
            var body = $"Dear {command.RecipientName},\n\nThank you for registering with us. We're excited to have you on board!\n\n- Bookstore Team";

            // Send email
            await _emailSender.SendEmailAsync(command.RecipientEmail, subject, body, ct);

            return new { };
        }
    }
}
