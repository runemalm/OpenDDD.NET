namespace DDD.Application.Settings
{
	public interface IPubSubSettings
	{
		PubSubProvider Provider { get; }
		int MaxDeliveryRetries { get; }
		bool PublisherEnabled { get; }
	}
}
