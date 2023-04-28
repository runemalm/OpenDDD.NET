namespace OpenDDD.Application.Settings.Email
{
	public interface IEmailSmtpSettings
	{
		string Host { get; }
		int Port { get; }
		string Username { get; }
		string Password { get; }
	}
}
