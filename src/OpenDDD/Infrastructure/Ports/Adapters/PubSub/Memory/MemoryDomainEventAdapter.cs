using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Monitoring;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Memory
{
	public class MemoryDomainEventAdapter : MemoryEventAdapter, IDomainEventAdapter
	{
		public MemoryDomainEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			ConversionSettings conversionSettings) :
			base(
				settings.General.Context,
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				logger,
				monitoringAdapter,
				conversionSettings)
		{
			
		}
	}
}
