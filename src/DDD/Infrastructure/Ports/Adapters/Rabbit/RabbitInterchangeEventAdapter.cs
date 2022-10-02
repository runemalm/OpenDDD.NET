using DDD.Logging;
using DDD.Application.Settings;

namespace DDD.Infrastructure.Ports.Adapters.Rabbit
{
	public class RabbitInterchangeEventAdapter : RabbitEventAdapter, IInterchangeEventAdapter
	{
		public RabbitInterchangeEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter) :
			base(
				"Interchange",
				settings.General.Context,
				settings.PubSub.MaxDeliveryRetries,
				settings.Rabbit.Host,
				settings.Rabbit.Port,
				logger,
				monitoringAdapter,
				settings.Rabbit.Username,
				settings.Rabbit.Password)
		{
			
		}
	}
}
