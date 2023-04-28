using OpenDDD.Domain;
using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Ports.PubSub
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
