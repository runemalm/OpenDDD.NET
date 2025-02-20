using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Events.Kafka.Factories;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace OpenDDD.Infrastructure.Events.Kafka
{
    public class KafkaMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly string _bootstrapServers;
        private readonly IProducer<Null, string> _producer;
        private readonly IAdminClient _adminClient;
        private readonly bool _autoCreateTopics;
        private readonly KafkaConsumerFactory _consumerFactory;
        private readonly ILogger<KafkaMessagingProvider> _logger;
        private readonly ConcurrentBag<IConsumer<Ignore, string>> _consumers = new();
        private readonly List<Task> _consumerTasks = new();
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        public KafkaMessagingProvider(
            string bootstrapServers,
            IAdminClient adminClient,
            IProducer<Null, string> producer,
            KafkaConsumerFactory consumerFactory,
            bool autoCreateTopics,
            ILogger<KafkaMessagingProvider> logger)
        {
            if (string.IsNullOrWhiteSpace(bootstrapServers))
                throw new ArgumentNullException(nameof(bootstrapServers));

            _bootstrapServers = bootstrapServers;
            _adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _consumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));
            _autoCreateTopics = autoCreateTopics;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task SubscribeAsync(
            string topic,
            string consumerGroup,
            Func<string, CancellationToken, Task> messageHandler,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(consumerGroup))
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));

            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));

            if (_autoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
            }

            var consumer = _consumerFactory.Create(consumerGroup);
            _consumers.Add(consumer);
            consumer.Subscribe(topic);

            _logger.LogDebug("Subscribed to Kafka topic '{Topic}' with consumer group '{ConsumerGroup}'", topic, consumerGroup);
            
            var consumerTask = Task.Run(() => StartConsumerLoop(consumer, messageHandler, _cts.Token), _cts.Token);
            _consumerTasks.Add(consumerTask);
        }
        
        private async Task StartConsumerLoop(
            IConsumer<Ignore, string> consumer, 
            Func<string, CancellationToken, Task> messageHandler, 
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Seems like consume don't respect cancellation token.
                    // See: https://github.com/confluentinc/confluent-kafka-dotnet/issues/1085
                    var result = consumer.Consume(cancellationToken);
                    if (result?.Message != null)
                    {
                        _logger.LogDebug("Received message from Kafka: {Message}", result.Message.Value);
                        await messageHandler(result.Message.Value, cancellationToken);
                        consumer.Commit(result);
                        _logger.LogDebug("Message processed and offset committed: {Offset}", result.Offset);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Kafka consumer loop cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Kafka consumer loop.");
            }
            finally
            {
                _logger.LogDebug("Closing consumer.");
                consumer.Close();
            }
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            if (_autoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
            }

            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
            _logger.LogDebug("Published message to Kafka topic '{Topic}'", topic);
        }
        
        private async Task CreateTopicIfNotExistsAsync(string topic, CancellationToken cancellationToken)
        {
            try
            {
                var metadata = _adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
                if (metadata.Topics.Any(t => t.Topic == topic)) return; // Topic exists

                _logger.LogDebug("Creating Kafka topic: {Topic}", topic);
                await _adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 }
                }, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not check or create Kafka topic {Topic}: {Message}", topic, ex.Message);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.LogDebug("Disposing KafkaMessagingProvider...");

            _logger.LogDebug("Cancelling consumer tasks...");
            _cts.Cancel();
            
            _logger.LogDebug("Waiting for all consumer tasks to complete...");
            await Task.WhenAll(_consumerTasks);

            foreach (var consumer in _consumers)
            {
                _logger.LogDebug("Disposing consumer...");
                consumer.Dispose();
            }

            _logger.LogDebug("Disposing producer...");
            _producer.Dispose();
        
            _logger.LogDebug("Disposing admin client...");
            _adminClient.Dispose();
        }
    }
}
