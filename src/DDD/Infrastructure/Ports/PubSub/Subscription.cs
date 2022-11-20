using DDD.Domain;
using DDD.Domain.Model;

namespace DDD.Infrastructure.Ports.PubSub
{
	public class Subscription
	{
		public string EventName;
		public DomainModelVersion DomainModelVersion;
		public IEventListener Listener;

		public Subscription(IEventListener listener)
		{
			EventName = listener.ListensTo;
			DomainModelVersion = listener.ListensToVersion;
			Listener = listener;
		}
	}
}
