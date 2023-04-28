using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.PubSub
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
