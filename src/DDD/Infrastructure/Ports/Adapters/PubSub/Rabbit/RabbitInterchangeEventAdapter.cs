using DDD.Application.Settings;
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
