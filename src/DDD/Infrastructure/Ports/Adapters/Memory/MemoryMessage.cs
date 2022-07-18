namespace DDD.Infrastructure.Ports.Adapters.Memory
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
