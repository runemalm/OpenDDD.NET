using OpenDDD.NET.Extensions;
using Microsoft.Extensions.Options;

namespace OpenDDD.Application.Settings.PubSub
{
	public class PubSubSettings : IPubSubSettings
	{
		public PubSubProvider Provider { get; set; }
		public int MaxDeliveryRetries { get; }
		public bool PublisherEnabled { get; }

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

			int.TryParse(options.Value.PUBSUB_MAX_DELIVERY_RETRIES, out var maxDeliveryRetries);

			var publisherEnabled = options.Value.PUBSUB_PUBLISHER_ENABLED.IsTrue();

			Provider = provider;
			MaxDeliveryRetries = maxDeliveryRetries;
			PublisherEnabled = publisherEnabled;
		}
	}
}
