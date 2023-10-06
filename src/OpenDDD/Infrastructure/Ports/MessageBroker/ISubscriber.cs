using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.MessageBroker
{
    public interface ISubscriber
    {
        IMessageBrokerConnection MessageBrokerConnection { get; set; }
        Topic Topic { get; set; }
        ConsumerGroup ConsumerGroup { get; set; }
        ISubscription? Subscription { get; set; }
        
        void Subscribe();
        Task SubscribeAsync();
        void Unsubscribe();
        Task UnsubscribeAsync();
    }
}
