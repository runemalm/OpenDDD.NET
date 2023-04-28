using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Monitoring;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Rabbit
{
	public class RabbitInterchangeEventAdapter : RabbitEventAdapter, IInterchangeEventAdapter
	{
		public RabbitInterchangeEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			ConversionSettings conversionSettings) :
			base(
				"Interchange",
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				settings.Rabbit.Host,
				settings.Rabbit.Port,
				logger,
				monitoringAdapter,
				conversionSettings,
				settings.Rabbit.Username,
				settings.Rabbit.Password)
		{
			
		}
	}
}
