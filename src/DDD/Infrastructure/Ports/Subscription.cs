using DDD.Domain;

namespace DDD.Infrastructure.Ports
{
	public class Subscription
	{
		public string EventName;
		public IDomainModelVersion DomainModelVersion;
		public IEventListener Listener;

		public Subscription(IEventListener listener)
		{
			EventName = listener.ListensTo;
			DomainModelVersion = listener.ListensToVersion;
			Listener = listener;
		}
	}
}
