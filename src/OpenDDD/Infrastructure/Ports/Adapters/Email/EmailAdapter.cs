using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Email;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.Email
{
	public abstract class EmailAdapter : IEmailPort
	{
		protected readonly ISettings _settings;
		protected readonly ILogger _logger;
		private List<IEmail> _sent = new List<IEmail>();
		protected bool _enabled;

		public EmailAdapter(ISettings settings, ILogger logger)
		{
			_settings = settings;
			_logger = logger;
			_enabled = true;
		}
		
		public abstract Task SendAsync(
			string fromEmail, string fromName, string toEmail, string toName, string subject,
			string message, bool isHtml, CancellationToken ct);

		public bool HasSent(string toEmail)
			=> HasSent(toEmail, null);

		public bool HasSent(string toEmail, string? msgContains)
			=> _sent.Any(s => s.ToEmail == toEmail && (msgContains == null || s.Message.Contains(msgContains)));

		public void Empty(CancellationToken ct)
		{
			_sent = new List<IEmail>();
		}
		
		public Task EmptyAsync(CancellationToken ct)
		{
			Empty(ct);
			return Task.CompletedTask;
		}

		protected Task AddToLog(IEmail email)
		{
			_sent.Add(email);
			return Task.CompletedTask;
		}

		public void SetEnabled(bool enabled)
		{
			_enabled = enabled;
		}
	}
}
