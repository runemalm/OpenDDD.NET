using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Ports.PubSub
{
	public class Subscription
	{
		public string EventName;
		public string ActionName;
		public DomainModelVersion DomainModelVersion;
		public IEventListener Listener;

		public Subscription(IEventListener listener)
		{
			EventName = listener.ListensTo;
			ActionName = listener.ActionName;
			DomainModelVersion = listener.ListensToVersion;
			Listener = listener;
		}
	}
}
