using Microsoft.Extensions.Options;
using OpenDDD.Application.Settings.Auth;
using OpenDDD.Application.Settings.Azure;
using OpenDDD.Application.Settings.Email;
using OpenDDD.Application.Settings.General;
using OpenDDD.Application.Settings.Http;
using OpenDDD.Application.Settings.Monitoring;
using OpenDDD.Application.Settings.Persistence;
using OpenDDD.Application.Settings.Postgres;
using OpenDDD.Application.Settings.PowerIam;
using OpenDDD.Application.Settings.PubSub;
using OpenDDD.Application.Settings.Rabbit;
using OpenDDD.Application.Settings.ServiceBus;

namespace OpenDDD.Application.Settings
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
		public IPowerIamSettings PowerIam { get; }
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
			PowerIam = new PowerIamSettings(options);
			Postgres = new PostgresSettings(options);
			PubSub = new PubSubSettings(options);
			Rabbit = new RabbitSettings(options);
		}
	}
}
