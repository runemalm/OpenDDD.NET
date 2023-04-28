using OpenDDD.Infrastructure.Ports.PubSub;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Memory
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
