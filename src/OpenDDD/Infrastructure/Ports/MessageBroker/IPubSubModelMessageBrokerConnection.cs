using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.MessageBroker
{
    public interface IPubSubModelMessageBrokerConnection
    {
        ISubscription Subscribe(Topic topic, ConsumerGroup consumerGroup);
        Task<ISubscription> SubscribeAsync(Topic topic, ConsumerGroup consumerGroup);
        Task PublishAsync(IMessage message, Topic topic);
    }
}
