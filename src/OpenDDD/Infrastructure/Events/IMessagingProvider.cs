namespace OpenDDD.Infrastructure.Events
{
	public interface IMessagingProvider
	{
		Task SubscribeAsync(string topic, Func<string, Task> messageHandler);
		Task PublishAsync(string topic, string message);
	}
}
