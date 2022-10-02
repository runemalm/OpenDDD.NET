using DDD.Logging;
using DDD.Application.Settings;

namespace DDD.Infrastructure.Ports.Adapters.Rabbit
{
	public class RabbitDomainEventAdapter : RabbitEventAdapter, IDomainEventAdapter
	{
		public RabbitDomainEventAdapter(
			ISettings settings,
			ILogger logger,
			IMonitoringPort monitoringAdapter) :
			base(
				settings.General.Context,
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
