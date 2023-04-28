using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Monitoring;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.ServiceBus
{
	public class ServiceBusDomainEventAdapter : ServiceBusEventAdapter, IDomainEventAdapter
	{
		public ServiceBusDomainEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			SerializerSettings serializerSettings) :
			base(
				settings.General.Context,
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				settings.Azure.ServiceBus.ConnString,
				settings.Azure.ServiceBus.SubName,
				logger,
				monitoringAdapter,
				serializerSettings)
		{
			
		}
	}
}
