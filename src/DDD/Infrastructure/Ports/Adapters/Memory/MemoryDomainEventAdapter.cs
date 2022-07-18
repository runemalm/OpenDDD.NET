using DDD.Logging;
using DDD.Application.Settings;

namespace DDD.Infrastructure.Ports.Adapters.Memory
{
	public class MemoryDomainEventAdapter : MemoryEventAdapter, IDomainEventAdapter
	{
		public MemoryDomainEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter) :
			base(
				settings.General.Context,
				settings.General.Context,
				settings.PubSub.ListenerAcksRequired,
				settings.PubSub.PublisherAcksRequired,
				logger,
				monitoringAdapter)
		{
			
		}
	}
}
