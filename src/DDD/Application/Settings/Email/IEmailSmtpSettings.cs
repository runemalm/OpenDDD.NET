namespace DDD.Application.Settings.Email
{
	public interface IEmailSmtpSettings
	{
		string Host { get; }
		int Port { get; }
	}
}
