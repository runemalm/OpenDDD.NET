using System;
using OpenDDD.NET.Extensions;
using Microsoft.Extensions.Options;

namespace OpenDDD.Application.Settings.Email
{
	public class EmailSmtpSettings : IEmailSmtpSettings
	{
		public string Host { get; }
		public int Port { get; }
		public string Username { get; }
		public string Password { get; }
		
		public EmailSmtpSettings() { }

		public EmailSmtpSettings(IOptions<Options> options)
		{
			Host = options.Value.EMAIL_SMTP_HOST;
			Port = options.Value.EMAIL_SMTP_PORT.IntValue();
			Username = options.Value.EMAIL_SMTP_USERNAME;
			Password = options.Value.EMAIL_SMTP_PASSWORD;
		}
	}
}
