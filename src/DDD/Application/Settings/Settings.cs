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
