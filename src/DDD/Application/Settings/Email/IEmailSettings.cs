namespace DDD.Application.Settings.Email
{
	public interface IEmailSettings
	{
		bool Enabled { get; }
		EmailProvider Provider { get; }
		IEmailSmtpSettings Smtp { get; }
	}
}
