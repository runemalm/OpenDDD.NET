namespace Bookstore.Domain.Model.Ports
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string recipient, string subject, string body, CancellationToken ct);
    }
}
