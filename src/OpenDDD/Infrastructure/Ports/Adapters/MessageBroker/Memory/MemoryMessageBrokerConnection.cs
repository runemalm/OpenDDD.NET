using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Ports.MessageBroker;

namespace OpenDDD.Infrastructure.Ports.Adapters.MessageBroker.Memory
{
    public class MemoryMessageBrokerConnection : BaseMessageBrokerConnection<IMemoryMessageBrokerConnectionSettings>, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IMemoryMessageBroker _messageBroker;
        private bool _isConnected;

        public MemoryMessageBrokerConnection(ILogger<MemoryMessageBrokerConnection> logger, IMemoryMessageBrokerConnectionSettings settings, IMemoryMessageBroker messageBroker) 
            : base(settings)
        {
            _logger = logger;
            _messageBroker = messageBroker;
        }
        
        // IDisposable

        public override void Dispose()
        {
            Stop(CancellationToken.None);
        }

        // IMessageBrokerConnection

        public override void Open()
        {
            if (!_isConnected)
            {
                _isConnected = true;
                _logger.LogDebug("Opening memory database connection.");
            }
        }

        public override Task OpenAsync()
        {
            Open();
            return Task.CompletedTask;
        }

        public override void Close()
        {
            if (_isConnected)
            {
                _isConnected = false;
                _logger.LogDebug("Closing memory database connection.");
            }
        }

        public override Task CloseAsync()
        {
            Close();
            return Task.CompletedTask;
        }

        // IPubSubModelMessageBrokerConnection
        
        public override ISubscription Subscribe(Topic topic, ConsumerGroup consumerGroup)
        {
            return _messageBroker.Subscribe(topic, consumerGroup);
        }

        public override Task<ISubscription> SubscribeAsync(Topic topic, ConsumerGroup consumerGroup)
        {
            return Task.FromResult(Subscribe(topic, consumerGroup));
        }
        
        public override async Task PublishAsync(IMessage message, Topic topic)
        {
            await _messageBroker.PublishAsync(message, topic);
        }
        
        // IStartable
        
        public override bool IsStarted { get; set; }
        public override void Start(CancellationToken ct)
        {
            Open();
            IsStarted = true;
        }

        public override Task StartAsync(CancellationToken ct, bool blocking = false)
        {
            Start(ct);
            return Task.CompletedTask;
        }

        public override void Stop(CancellationToken ct)
        {
            Close();
            IsStarted = false;
        }

        public override Task StopAsync(CancellationToken ct, bool blocking = false)
        {
            Stop(ct);
            return Task.CompletedTask;
        }
        
        // Helpers
        
        public void AssureConnected()
        {
            if (!_isConnected)
                throw new ApplicationException("Can't perform operation, not connected.");
        }
    }
}
