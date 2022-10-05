using DDD.Infrastructure.Ports.PubSub;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.Memory
{
	public class MemoryMessage : IPubSubMessage
	{
		public string Message;

		public MemoryMessage(string message)
		{
			Message = message;
		}

		public override string ToString()
			=> Message;
	}
}
