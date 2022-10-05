using DDD.Logging;

namespace DDD.Infrastructure.Ports.PubSub
{
	public class DomainPublisher : Publisher, IDomainPublisher
	{
		public DomainPublisher(
			IDomainEventAdapter eventAdapter,
			ILogger logger) :
			base(
				eventAdapter,
				logger)
		{
			
		}
	}
}
