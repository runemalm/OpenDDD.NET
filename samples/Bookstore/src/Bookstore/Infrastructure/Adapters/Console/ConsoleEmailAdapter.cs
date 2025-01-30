using Bookstore.Domain.Model.Ports;

namespace Bookstore.Infrastructure.Adapters.Console
{
    public class ConsoleEmailAdapter : IEmailPort
    {
        private readonly ILogger<ConsoleEmailAdapter> _logger;

        public ConsoleEmailAdapter(ILogger<ConsoleEmailAdapter> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct)
        {
            _logger.LogInformation($"Sending email to {to}");
            return Task.CompletedTask;
        }
    }
}
