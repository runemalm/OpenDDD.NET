using DDD.Application.Settings;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Monitoring;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.Memory
{
	public class MemoryDomainEventAdapter : MemoryEventAdapter, IDomainEventAdapter
	{
		public MemoryDomainEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			SerializerSettings serializerSettings) :
			base(
				settings.General.Context,
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				logger,
				monitoringAdapter,
				serializerSettings)
		{
			
		}
	}
}
