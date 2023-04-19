using System.Threading;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Ports.Email
{
	public interface IEmailPort
	{
		Task SendAsync(string fromEmail, string fromName, string toEmail, string toName, string subject, string message, bool isHtml, CancellationToken ct);
		bool HasSent(string toEmail);
		bool HasSent(string toEmail, string? msgContains);
		Task EmptyAsync(CancellationToken ct);
	}
}
