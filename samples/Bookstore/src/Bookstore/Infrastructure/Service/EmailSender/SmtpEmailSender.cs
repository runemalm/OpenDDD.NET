using OpenDDD.Infrastructure.Services;
using Bookstore.Domain.Model.Ports;

namespace Bookstore.Infrastructure.Service.EmailSender
{
    public class SmtpEmailSender : IEmailSender, IInfrastructureService
    {
        public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
