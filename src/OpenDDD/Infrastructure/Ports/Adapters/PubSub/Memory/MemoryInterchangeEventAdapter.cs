using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Monitoring;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Memory
{
	public class MemoryInterchangeEventAdapter : MemoryEventAdapter, IInterchangeEventAdapter
	{
		public MemoryInterchangeEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			SerializerSettings serializerSettings) :
			base(
				"Interchange",
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				logger,
				monitoringAdapter,
				serializerSettings)
		{
			
		}
	}
}
