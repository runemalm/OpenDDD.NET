using DDD.Logging;
using DDD.Application.Settings;

namespace DDD.Infrastructure.Ports.Adapters.Memory
{
	public class MemoryInterchangeEventAdapter : MemoryEventAdapter, IInterchangeEventAdapter
	{
		public MemoryInterchangeEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter) :
			base(
				"Interchange",
				settings.General.Context,
				settings.PubSub.ListenerAcksRequired,
				settings.PubSub.PublisherAcksRequired,
				logger,
				monitoringAdapter)
		{
			
		}
	}
}
