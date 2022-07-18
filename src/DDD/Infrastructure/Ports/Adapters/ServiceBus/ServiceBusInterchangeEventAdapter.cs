using DDD.Logging;
using DDD.Application.Settings;

namespace DDD.Infrastructure.Ports.Adapters.ServiceBus
{
	public class ServiceBusInterchangeEventAdapter : ServiceBusEventAdapter, IInterchangeEventAdapter
	{
		public ServiceBusInterchangeEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter) :
			base(
				"Interchange",
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
