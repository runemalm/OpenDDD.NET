namespace DDD.Application.Settings
{
	public interface IEmailSmtpSettings
	{
		string Host { get; }
		int Port { get; }
	}
}
