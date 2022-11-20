using DDD.NETCore.Extensions;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.Email
{
	public class EmailSettings : IEmailSettings
	{
		public bool Enabled { get; set; }
		public EmailProvider Provider { get; set; }
		public IEmailSmtpSettings Smtp { get; }
		
		public EmailSettings() { }

		public EmailSettings(IOptions<Options> options)
		{
			var enabled = options.Value.EMAIL_ENABLED.IsTrue();
			
			var provider = EmailProvider.None;
			var providerString = options.Value.EMAIL_PROVIDER;
			if (providerString != null)
				if (providerString.ToLower() == "smtp")
					provider = EmailProvider.Smtp;
				else if (providerString.ToLower() == "memory")
					provider = EmailProvider.Memory;

			Enabled = enabled;
			Provider = provider;
			Smtp = new EmailSmtpSettings(options);
		}
	}
}
