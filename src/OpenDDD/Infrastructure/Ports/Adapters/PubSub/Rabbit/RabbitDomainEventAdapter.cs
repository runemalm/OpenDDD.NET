using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Monitoring;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Rabbit
{
	public class RabbitDomainEventAdapter : RabbitEventAdapter, IDomainEventAdapter
	{
		public RabbitDomainEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			SerializerSettings serializerSettings) :
			base(
				settings.General.Context,
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				settings.Rabbit.Host,
				settings.Rabbit.Port,
				logger,
				monitoringAdapter,
				serializerSettings,
				settings.Rabbit.Username,
				settings.Rabbit.Password)
		{
			
		}
	}
}
