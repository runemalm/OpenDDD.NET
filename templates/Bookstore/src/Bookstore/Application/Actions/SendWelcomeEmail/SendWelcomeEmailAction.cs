using OpenDDD.Application;
using Bookstore.Domain.Model.Ports;

namespace Bookstore.Application.Actions.SendWelcomeEmail
{
    public class SendWelcomeEmailAction : IAction<SendWelcomeEmailCommand, object>
    {
        private readonly IEmailPort _emailPort;

        public SendWelcomeEmailAction(IEmailPort emailPort)
        {
            _emailPort = emailPort;
        }

        public async Task<object> ExecuteAsync(SendWelcomeEmailCommand command, CancellationToken ct)
        {
            var subject = "Welcome to Bookstore!";
            var body = $"Dear {command.RecipientName},\n\nThank you for registering with us. We're excited to have you on board!\n\n- Bookstore Team";

            // Send email
            await _emailPort.SendEmailAsync(command.RecipientEmail, subject, body, ct);

            return new { };
        }
    }
}
