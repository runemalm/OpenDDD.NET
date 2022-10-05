using DDD.Application.Settings;
using DDD.Infrastructure.Ports.Monitoring;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.ServiceBus
{
	public class ServiceBusDomainEventAdapter : ServiceBusEventAdapter, IDomainEventAdapter
	{
		public ServiceBusDomainEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter) :
			base(
				settings.General.Context,
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				settings.Azure.ServiceBus.ConnString,
				settings.Azure.ServiceBus.SubName,
				logger,
				monitoringAdapter)
		{
			
		}
	}
}
