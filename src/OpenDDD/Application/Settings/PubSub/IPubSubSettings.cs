namespace OpenDDD.Application.Settings.PubSub
{
	public interface IPubSubSettings
	{
		PubSubProvider Provider { get; }
		int MaxDeliveryRetries { get; }
		bool PublisherEnabled { get; }
	}
}
