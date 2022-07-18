using DDD.Logging;
using DDD.Application.Settings;

namespace DDD.Infrastructure.Ports.Adapters.ServiceBus
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
				settings.PubSub.ListenerAcksRequired,
				settings.PubSub.PublisherAcksRequired,
				settings.Azure.ServiceBus.ConnString,
				settings.Azure.ServiceBus.SubName,
				logger,
				monitoringAdapter)
		{
			
		}
	}
}
