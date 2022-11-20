using DDD.Application.Settings.Auth;
using DDD.Application.Settings.Azure;
using DDD.Application.Settings.Email;
using DDD.Application.Settings.General;
using DDD.Application.Settings.Http;
using DDD.Application.Settings.Monitoring;
using DDD.Application.Settings.Persistence;
using DDD.Application.Settings.Postgres;
using DDD.Application.Settings.PubSub;
using DDD.Application.Settings.Rabbit;
using DDD.Application.Settings.ServiceBus;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings
{
	public class Settings : ISettings
	{
		public IAuthSettings Auth { get; }
		public IAzureSettings Azure { get; }
		public IEmailSettings Email { get; }
		public IGeneralSettings General { get; }
		public IHttpSettings Http { get; }
		public IMonitoringSettings Monitoring { get; }
		public IPersistenceSettings Persistence { get; }
		public IPostgresSettings Postgres { get; }
		public IPubSubSettings PubSub { get; }
		public IRabbitSettings Rabbit { get; }

		public Settings(IOptions<Options> options)
		{
			Auth = new AuthSettings(options);
			Azure = new AzureSettings(new ServiceBusSettings(options));    // TODO: Normalize constructor
			Email = new EmailSettings(options);
			General = new GeneralSettings(options);
			Http = new HttpSettings(options);
			Monitoring = new MonitoringSettings(options);
			Persistence = new PersistenceSettings(options);
			Postgres = new PostgresSettings(options);
			PubSub = new PubSubSettings(options);
			Rabbit = new RabbitSettings(options);
		}
	}
}
