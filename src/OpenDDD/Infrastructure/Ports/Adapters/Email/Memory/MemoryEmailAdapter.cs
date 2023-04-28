using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application.Settings;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.Email.Memory
{
	public class MemoryEmailAdapter : EmailAdapter
	{
		public MemoryEmailAdapter(ISettings settings, ILogger logger) : base(settings, logger)
		{
			
		}

		public override Task SendAsync(
			string fromEmail, string fromName, string toEmail, string toName, 
			string subject, string message, bool isHtml, CancellationToken ct)
		{
			if (!_enabled)
				return Task.CompletedTask;

			AddToLog(MemoryEmail.Create(toEmail, message));
			return Task.CompletedTask;
		}
	}
}
