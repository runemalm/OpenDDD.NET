namespace DDD.Application.Settings
{
	public interface IEmailSettings
	{
		bool Enabled { get; }
		EmailProvider Provider { get; }
		IEmailSmtpSettings Smtp { get; }
	}
}
