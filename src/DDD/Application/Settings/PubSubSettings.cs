using DDD.DotNet.Extensions;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings
{
	public class PubSubSettings : IPubSubSettings
	{
		public PubSubProvider Provider { get; set; }
		public bool ListenerAcksRequired { get; }
		public bool PublisherAcksRequired { get; }

		public PubSubSettings() { }

		public PubSubSettings(IOptions<Options> options)
		{
			var provider = PubSubProvider.None;
			var providerString = options.Value.PUBSUB_PROVIDER;
			if (providerString != null)
				if (providerString.ToLower() == "memory")
					provider = PubSubProvider.Memory;
				else if (providerString.ToLower() == "rabbit")
					provider = PubSubProvider.Rabbit;
				else if (providerString.ToLower() == "servicebus")
					provider = PubSubProvider.ServiceBus;

			var listenerAcksRequired = options.Value.PUBSUB_LISTENER_ACKS_REQUIRED.IsTrue();
			var publisherAcksRequired = options.Value.PUBSUB_PUBLISHER_ACKS_REQUIRED.IsTrue();

			Provider = provider;
			ListenerAcksRequired = listenerAcksRequired;
			PublisherAcksRequired = publisherAcksRequired;
		}
	}
}
