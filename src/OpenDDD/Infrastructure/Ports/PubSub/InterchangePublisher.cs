using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.PubSub
{
	public class InterchangePublisher : Publisher, IInterchangePublisher
	{
		public InterchangePublisher(
			IInterchangeEventAdapter eventAdapter,
			ILogger logger) :
			base(
				eventAdapter,
				logger)
		{
			
		}
	}
}
