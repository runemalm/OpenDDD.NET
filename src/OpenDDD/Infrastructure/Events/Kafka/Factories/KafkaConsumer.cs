using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace OpenDDD.Infrastructure.Events.Kafka.Factories
{
    public class KafkaConsumer : IDisposable
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly CancellationTokenSource _cts = new();
        private Task? _consumerTask;
        private bool _disposed;

        public string ConsumerGroup { get; }
        public HashSet<string> SubscribedTopics { get; } = new();

        public KafkaConsumer(IConsumer<Ignore, string> consumer, string consumerGroup, ILogger<KafkaConsumer> logger)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            ConsumerGroup = consumerGroup ?? throw new ArgumentNullException(nameof(consumerGroup));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Subscribe(string topic)
        {
            if (SubscribedTopics.Contains(topic)) return;

            _consumer.Subscribe(topic);
            SubscribedTopics.Add(topic);
        }
        
        public void StartProcessing(Func<string, CancellationToken, Task> messageHandler, CancellationToken globalToken)
        {
            if (_consumerTask != null) return;

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken, _cts.Token);
            _consumerTask = Task.Run(() => ConsumeLoop(messageHandler, linkedCts.Token), linkedCts.Token);
        }

        private async Task ConsumeLoop(Func<string, CancellationToken, Task> messageHandler, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? result = null;

                try
                {
                    result = _consumer.Consume(token);
                    if (result?.Message == null) continue;

                    await messageHandler(result.Message.Value, token);
                    _consumer.Commit(result);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Kafka consumer loop. Retrying...");
                    
                    try
                    {
                        await Task.Delay(5000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }

                    if (result != null)
                    {
                        _consumer.Seek(new TopicPartitionOffset(result.TopicPartition, result.Offset));
                    }
                }
            }
        }

        public async Task StopProcessingAsync()
        {
            _cts.Cancel();
            
            if (_consumerTask != null)
            {
                await _consumerTask;
                _consumerTask = null;
            }
            
            _consumer.Close();
            _consumer.Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _consumer.Close();
            }
            catch (ObjectDisposedException)
            {
                _logger.LogWarning("Attempted to close a disposed Kafka consumer.");
            }

            _consumer.Dispose();
            _cts.Dispose();
        }
    }
}
