namespace Bookstore.Domain.Model.Ports
{
    public interface IEmailSender
    {
        void SendEmail(string recipient, string subject, string body);
    }
}
