using Bookstore.Domain.Model.Ports;
using OpenDDD.Infrastructure.Services;

namespace Bookstore.Infrastructure.Service.EmailSender
{
    public class SmtpEmailSender : IEmailSender, IInfrastructureService
    {
        public void SendEmail(string recipient, string subject, string body)
        {
            throw new NotImplementedException();
        }
    }
}
