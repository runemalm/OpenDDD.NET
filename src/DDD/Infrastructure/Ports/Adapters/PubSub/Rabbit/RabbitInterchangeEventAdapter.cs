using DDD.Application.Settings;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Monitoring;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.Rabbit
{
	public class RabbitInterchangeEventAdapter : RabbitEventAdapter, IInterchangeEventAdapter
	{
		public RabbitInterchangeEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			SerializerSettings serializerSettings) :
			base(
				"Interchange",
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				settings.Rabbit.Host,
				settings.Rabbit.Port,
				logger,
				monitoringAdapter,
				serializerSettings,
				settings.Rabbit.Username,
				settings.Rabbit.Password)
		{
			
		}
	}
}
