using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.MessageBroker
{
    public class Subscription : ISubscription
    {
        public Topic Topic { get; set; }
        public ConsumerGroup ConsumerGroup { get; set; }

        public Subscription(Topic topic, ConsumerGroup consumerGroup)
        {
            Topic = topic;
            ConsumerGroup = consumerGroup;
        }
        
        // Events
        
        public event ISubscription.ReceivedEventHandlerAsync? Received;
        public async Task OnReceivedAsync(ISubscription.ReceivedEventArgs eventArgs)
        {
            if (Received != null)
            {
                await Received(this, eventArgs);
            }
        }
    }
}
