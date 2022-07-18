namespace DDD.Application.Settings
{
	public interface ISettings
	{
		IAuthSettings Auth { get; }
		IAzureSettings Azure { get; }
		IEmailSettings Email { get; }
		IGeneralSettings General { get; }
		IHttpSettings Http { get; }
		IMonitoringSettings Monitoring { get; }
		IPersistenceSettings Persistence { get; }
		IPostgresSettings Postgres { get; }
		IPubSubSettings PubSub { get; }
		IRabbitSettings Rabbit { get; }
	}
}
