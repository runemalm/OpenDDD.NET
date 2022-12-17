using System.Threading;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Ports.Email
{
	public interface IEmailPort
	{
		Task SendAsync(string fromEmail, string fromName, string toEmail, string toName, string subject, string message, bool isHtml, CancellationToken ct);
	}
}
