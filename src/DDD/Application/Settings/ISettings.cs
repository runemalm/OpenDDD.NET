using DDD.Application.Settings.Auth;
using DDD.Application.Settings.Azure;
using DDD.Application.Settings.Email;
using DDD.Application.Settings.General;
using DDD.Application.Settings.Http;
using DDD.Application.Settings.Monitoring;
using DDD.Application.Settings.Persistence;
using DDD.Application.Settings.Postgres;
using DDD.Application.Settings.PowerIam;
using DDD.Application.Settings.PubSub;
using DDD.Application.Settings.Rabbit;

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
		IPowerIamSettings PowerIam { get; }
		IPostgresSettings Postgres { get; }
		IPubSubSettings PubSub { get; }
		IRabbitSettings Rabbit { get; }
	}
}
