using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Infrastructure.Events.RabbitMq.Factories;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace OpenDDD.Infrastructure.Events.RabbitMq
{
    public class RabbitMqMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IRabbitMqConsumerFactory _consumerFactory;
        private readonly ILogger<RabbitMqMessagingProvider> _logger;
        private readonly bool _autoCreateTopics;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly ConcurrentDictionary<string, RabbitMqSubscription> _subscriptions = new();
        private readonly ConcurrentDictionary<string, DateTime> _topicCache = new();
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(600);
        private bool _disposed;
        
        public RabbitMqMessagingProvider(
            IConnectionFactory factory,
            IRabbitMqConsumerFactory consumerFactory,
            bool autoCreateTopics,
            ILogger<RabbitMqMessagingProvider> logger)
        {
            _connectionFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            _consumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));
            _autoCreateTopics = autoCreateTopics;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ISubscription> SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(consumerGroup))
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));

            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler), "Message handler cannot be null.");

            await EnsureConnectedAsync(cancellationToken);
            await EnsureTopicExistsAsync(topic, cancellationToken);
            
            var queueName = $"{consumerGroup}.{topic}";
            await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queueName, topic, "", cancellationToken: cancellationToken);

            var consumer = _consumerFactory.CreateConsumer(_channel, messageHandler);
            await consumer.StartConsumingAsync(queueName, cancellationToken);

            var subscription = new RabbitMqSubscription(topic, consumerGroup, consumer);
            _subscriptions[subscription.Id] = subscription;

            _logger.LogDebug("Subscribed to RabbitMQ topic '{Topic}' with consumer group '{ConsumerGroup}', Subscription ID: {SubscriptionId}", topic, consumerGroup, subscription.Id);

            return subscription;
        }

        public async Task UnsubscribeAsync(ISubscription subscription, CancellationToken cancellationToken = default)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            if (subscription is not RabbitMqSubscription rabbitSubscription || !_subscriptions.TryRemove(rabbitSubscription.Id, out _))
            {
                _logger.LogWarning("No active subscription found with ID {SubscriptionId}", subscription.Id);
                return;
            }

            _logger.LogDebug("Unsubscribing from RabbitMQ topic '{Topic}' with consumer group '{ConsumerGroup}', Subscription ID: {SubscriptionId}", 
                rabbitSubscription.Topic, rabbitSubscription.ConsumerGroup, rabbitSubscription.Id);

            await rabbitSubscription.Consumer.StopConsumingAsync(cancellationToken);
            await rabbitSubscription.DisposeAsync();
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            await EnsureConnectedAsync(cancellationToken);
            await EnsureTopicExistsAsync(topic, cancellationToken);

            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(topic, "", body, cancellationToken: cancellationToken);

            _logger.LogDebug("Published message to topic '{Topic}'", topic);
        }
        
        private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true }) return;

            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(null, cancellationToken);
        }
        
        private async Task EnsureTopicExistsAsync(string topic, CancellationToken cancellationToken)
        {
            if (_topicCache.TryGetValue(topic, out var lastChecked) && DateTime.UtcNow - lastChecked < _cacheExpiration)
            {
                _logger.LogDebug("Skipping exchange check for '{Topic}' (cached result).", topic);
                return;
            }

            bool exchangeExists = await ExchangeExistsAsync(topic, cancellationToken);
    
            if (exchangeExists)
            {
                _topicCache[topic] = DateTime.UtcNow;
                return;
            }

            if (_autoCreateTopics)
            {
                await _channel.ExchangeDeclareAsync(topic, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancellationToken);
                _logger.LogInformation("Auto-created exchange (topic): {Topic}", topic);
                _topicCache[topic] = DateTime.UtcNow;
                return;
            }
            
            throw new InvalidOperationException($"Topic '{topic}' does not exist. Enable 'autoCreateTopics' to create topics automatically.");
        }
        
        private async Task<bool> ExchangeExistsAsync(string exchange, CancellationToken cancellationToken)
        {
            try
            {
                await _channel!.ExchangeDeclarePassiveAsync(exchange, cancellationToken);
                return true;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
            {
                _logger.LogDebug("Exchange '{Exchange}' does not exist.", exchange);

                await EnsureConnectedAsync(cancellationToken);
        
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while checking if exchange '{Exchange}' exists.", exchange);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.LogDebug("Disposing RabbitMqMessagingProvider...");

            foreach (var subscription in _subscriptions.Values)
            {
                await UnsubscribeAsync(subscription);
            }

            _subscriptions.Clear();

            if (_channel is not null)
            {
                _logger.LogDebug("Disposing RabbitMQ channel...");
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                _logger.LogDebug("Disposing RabbitMQ connection...");
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}
