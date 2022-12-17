using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application.Settings;
using DDD.Infrastructure.Ports.Email;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.Email.Smtp
{
	public class SmtpEmailAdapter : IEmailPort
	{
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly SmtpClient _client;

		public SmtpEmailAdapter(ISettings settings, ILogger logger)
		{
			_settings = settings;
			_logger = logger;
			_client = CreateClient();
		}

		public async Task SendAsync(
			string fromEmail, string fromName, string toEmail, string toName, 
			string subject, string message, bool isHtml, CancellationToken ct)
		{
			MailAddress from = new MailAddress(fromEmail, fromName, Encoding.UTF8);
			MailAddress to = new MailAddress(toEmail, toName, Encoding.UTF8);
			MailMessage msg = new MailMessage(from, to);
			msg.Body = message;
			msg.BodyEncoding =  Encoding.UTF8;
			msg.Subject = subject;
			msg.SubjectEncoding = Encoding.UTF8;
			msg.IsBodyHtml = isHtml;
			
			await _client.SendMailAsync(msg);

			msg.Dispose();
		}
		
		private SmtpClient CreateClient()
		{
			return new SmtpClient(_settings.Email.Smtp.Host, _settings.Email.Smtp.Port);
		}
	}
}
