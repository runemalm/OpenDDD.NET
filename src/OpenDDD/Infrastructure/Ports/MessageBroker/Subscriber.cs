using System;
using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.MessageBroker
{
	public class Subscriber: ISubscriber
	{
		public Subscriber(IMessageBrokerConnection messageBrokerConnection, Topic topic, ConsumerGroup consumerGroup)
		{
			MessageBrokerConnection = messageBrokerConnection;
			Topic = topic;
			ConsumerGroup = consumerGroup;
		}

		// ISubscriber

		public IMessageBrokerConnection MessageBrokerConnection { get; set; }
		public Topic Topic { get; set; }
		public ConsumerGroup ConsumerGroup { get; set; }
		public ISubscription? Subscription { get; set; }

		public void Subscribe()
		{
			Subscription = MessageBrokerConnection.Subscribe(Topic, ConsumerGroup);
		}

		public Task SubscribeAsync()
		{
			Subscribe();
			return Task.CompletedTask;
		}

		public void Unsubscribe()
		{
			// TODO: Implement
			// MessageBrokerConnection.Unsubscribe(Subscription);
			// Subscription = null;
		}

		public Task UnsubscribeAsync()
		{
			Unsubscribe();
			return Task.CompletedTask;
		}
	}
}
