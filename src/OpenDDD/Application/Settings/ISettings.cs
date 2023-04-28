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

namespace OpenDDD.Application.Settings
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
