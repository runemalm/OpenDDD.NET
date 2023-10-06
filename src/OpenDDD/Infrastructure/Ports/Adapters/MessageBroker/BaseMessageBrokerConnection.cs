using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Infrastructure.Ports.MessageBroker;

namespace OpenDDD.Infrastructure.Ports.Adapters.MessageBroker
{
    public abstract class BaseMessageBrokerConnection<TSettings> : IMessageBrokerConnection, IPubSubModelMessageBrokerConnection 
        where TSettings : IMessageBrokerConnectionSettings
    {
        public IMessageBrokerConnectionSettings Settings { get; set; }
        
        public BaseMessageBrokerConnection(TSettings settings)
        {
            Settings = settings;
        }
        
        // IMessageBrokerConnection

        public abstract void Open();
        public abstract Task OpenAsync();
        public abstract void Close();
        public abstract Task CloseAsync();
        
        // IPubSubModelMessageBrokerConnection
        public abstract ISubscription Subscribe(Topic topic, ConsumerGroup consumerGroup);
        public abstract Task<ISubscription> SubscribeAsync(Topic topic, ConsumerGroup consumerGroup);
        public abstract Task PublishAsync(IMessage message, Topic topic);
        
        // IStartable
        
        public abstract bool IsStarted { get; set; }
        public abstract void Start(CancellationToken ct);
        public abstract Task StartAsync(CancellationToken ct, bool blocking = false);
        public abstract void Stop(CancellationToken ct);
        public abstract Task StopAsync(CancellationToken ct, bool blocking = false);
        
        // IDisposable
        public abstract void Dispose();
    }
}
