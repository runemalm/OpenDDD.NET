using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.Rabbit
{
	public class RabbitSettings : IRabbitSettings
	{
		public string Host { get; }
		public int Port { get; }
		public string Username { get; }
		public string Password { get; }

		public RabbitSettings() { }

		public RabbitSettings(IOptions<Options> options)
		{
			var host = options.Value.RABBIT_HOST;
			int.TryParse(options.Value.RABBIT_PORT, out var port);

			var username = options.Value.RABBIT_USERNAME;
			var password = options.Value.RABBIT_PASSWORD;

			Host = host;
			Port = port;
			Username = username;
			Password = password;
		}
	}
}
