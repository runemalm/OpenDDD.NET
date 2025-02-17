using OpenDDD.Domain.Model.Ports;

namespace Bookstore.Domain.Model.Ports
{
    public interface IEmailPort : IPort
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken ct);
    }
}
