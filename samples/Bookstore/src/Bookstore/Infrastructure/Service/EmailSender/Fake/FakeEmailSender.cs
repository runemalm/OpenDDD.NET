using OpenDDD.Infrastructure.Services;
using Bookstore.Domain.Model.Ports;

namespace Bookstore.Infrastructure.Service.EmailSender.Fake
{
    public class FakeEmailSender : IEmailSender, IInfrastructureService
    {
        private readonly ILogger _logger;
        
        public FakeEmailSender(ILogger<FakeEmailSender> logger)
        {
            _logger = logger;
        }
        
        public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken ct)
        {
            _logger.LogInformation($"Sending email to {recipient} (fake)");
            return Task.CompletedTask;
        }
    }
}
