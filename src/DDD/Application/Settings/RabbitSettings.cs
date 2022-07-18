using System;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings
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
			int port;
			Int32.TryParse(options.Value.RABBIT_PORT, out port);

			var username = options.Value.RABBIT_USERNAME;
			var password = options.Value.RABBIT_PASSWORD;

			Host = host;
			Port = port;
			Username = username;
			Password = password;
		}
	}
}
