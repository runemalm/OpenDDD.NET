namespace DDD.Application.Settings
{
	public interface IPubSubSettings
	{
		PubSubProvider Provider { get; }
		bool ListenerAcksRequired { get; }
		bool PublisherAcksRequired { get; }
	}
}
