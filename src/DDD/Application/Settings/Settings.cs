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

		public Settings(
			IAuthSettings authSettings,
			IAzureSettings azureSettings,
			IEmailSettings emailSettings,
			IGeneralSettings generalSettings,
			IHttpSettings httpSettings,
			IMonitoringSettings monitoringSettings,
			IPersistenceSettings persistenceSettings,
			IPostgresSettings postgresSettings,
			IPubSubSettings pubSubSettings,
			IRabbitSettings rabbitSettings)
		{
			Auth = authSettings;
			Azure = azureSettings;
			Email = emailSettings;
			General = generalSettings;
			Http = httpSettings;
			Monitoring = monitoringSettings;
			Persistence = persistenceSettings;
			Postgres = postgresSettings;
			PubSub = pubSubSettings;
			Rabbit = rabbitSettings;
		}
	}
}
