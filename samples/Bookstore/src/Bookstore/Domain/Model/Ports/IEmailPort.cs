namespace Bookstore.Domain.Model.Ports
{
    public interface IEmailPort
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken ct);
    }
}
