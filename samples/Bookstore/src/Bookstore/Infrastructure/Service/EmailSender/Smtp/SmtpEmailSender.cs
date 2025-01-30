using OpenDDD.Infrastructure.Service;
using Bookstore.Domain.Model.Ports;

namespace Bookstore.Infrastructure.Service.EmailSender.Smtp
{
    public class SmtpEmailSender : IEmailSender, IInfrastructureService
    {
        public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
