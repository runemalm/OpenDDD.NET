namespace DDD.Application.Settings
{
	public interface IRabbitSettings
	{
		string Host { get; }
		int Port { get; }
		string Username { get; }
		string Password { get; }
	}
}
