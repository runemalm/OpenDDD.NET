using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Ports.MessageBroker;

namespace OpenDDD.Infrastructure.Ports.Adapters.MessageBroker.Memory
{
    public class MemoryMessageBroker : IMemoryMessageBroker
    {
        private readonly ILogger _logger;
        private IDictionary<Topic, ICollection<ISubscription>> _subscriptions;

        public MemoryMessageBroker(ILogger<MemoryMessageBroker> logger)
        {
            _logger = logger;
            _subscriptions = new Dictionary<Topic, ICollection<ISubscription>>();
        }

        // IPubSubModelMessageBroker

        public ISubscription Subscribe(Topic topic, ConsumerGroup consumerGroup)
        {
            if (!_subscriptions.ContainsKey(topic))
            {
                _subscriptions[topic] = new List<ISubscription>();
            }

            var subscription = new Subscription(topic, consumerGroup);

            _subscriptions[topic].Add(subscription);

            return subscription;
        }

        public Task<ISubscription> SubscribeAsync(Topic topic, ConsumerGroup consumerGroup)
        {
            return Task.FromResult(Subscribe(topic, consumerGroup));
        }
        
        public async Task PublishAsync(IMessage message, Topic topic)
        {
            _subscriptions.TryGetValue(topic, out ICollection<ISubscription>? subscriptions);
                
            if (subscriptions != null)
            {
                var onePerConsumerGroup = subscriptions
                    .GroupBy(s => s.ConsumerGroup)
                    .Select(group => group.First()).ToList();
                
                foreach (var subscription in onePerConsumerGroup)
                {
                    var eventArgs = new ISubscription.ReceivedEventArgs(message);
                    await subscription.OnReceivedAsync(eventArgs);
                }
            }
        }
    }
}
