using System;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.Email
{
	public class EmailSmtpSettings : IEmailSmtpSettings
	{
		public string Host { get; }
		public int Port { get; }
		
		public EmailSmtpSettings() { }

		public EmailSmtpSettings(IOptions<Options> options)
		{
			var host = options.Value.EMAIL_SMTP_HOST;
			int port;
			Int32.TryParse(options.Value.EMAIL_SMTP_PORT, out port);

			Host = host;
			Port = port;
		}
	}
}
