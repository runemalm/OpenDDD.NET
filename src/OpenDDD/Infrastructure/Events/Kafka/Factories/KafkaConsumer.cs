using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace OpenDDD.Infrastructure.Events.Kafka.Factories
{
    public class KafkaConsumer : IAsyncDisposable
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
            
            // Make sure all partitions have initial commit offset
            await CommitInitialOffsetsAsync();
            
            _consumer.Close();
            _consumer.Dispose();
        }

        private async Task CommitInitialOffsetsAsync()
        {
            foreach (var partition in _consumer.Assignment)
            {
                // Check if an offset is already committed
                var committedOffsets = _consumer.Committed(new[] { partition }, TimeSpan.FromSeconds(5));
                var currentOffset = committedOffsets?.FirstOrDefault()?.Offset;

                if (currentOffset == null || currentOffset == Offset.Unset)
                {
                    // Query watermark offsets (LOW = earliest, HIGH = next available offset)
                    var watermarkOffsets = _consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(5));

                    if (watermarkOffsets.High == 0) // No messages ever written
                    {
                        _logger.LogDebug("Partition {Partition} has no messages. Sending placeholder message.", partition);

                        // Publish a placeholder message to the **specific partition**
                        using var producer = new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = "localhost:9092" }).Build();
                        var deliveryResult = await producer.ProduceAsync(
                            new TopicPartition(partition.Topic, partition.Partition), // Ensure correct partition
                            new Message<Null, string> { Value = "__init__" });

                        producer.Flush(TimeSpan.FromSeconds(5));

                        _logger.LogDebug("Sent __init__ message to partition {Partition}. Offset: {Offset}", partition, deliveryResult.Offset);

                        // Poll Kafka until watermark updates or timeout occurs
                        var timeout = TimeSpan.FromSeconds(5);
                        var startTime = DateTime.UtcNow;

                        while (DateTime.UtcNow - startTime < timeout)
                        {
                            await Task.Delay(100); // Small delay to avoid excessive polling
                            watermarkOffsets = _consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(5));

                            if (watermarkOffsets.High > 0) // Message registered
                                break;
                        }

                        if (watermarkOffsets.High == 0)
                        {
                            throw new TimeoutException($"Kafka did not register the __init__ message for partition {partition} within 5 seconds.");
                        }

                        // Ensure the new high watermark is exactly 1
                        var initialOffset = watermarkOffsets.High;
                        if (initialOffset.Value != 1)
                        {
                            throw new Exception($"Expected initial offset to be 1, but got {initialOffset.Value}");
                        }

                        _consumer.Commit(new[] { new TopicPartitionOffset(partition, initialOffset) });

                        _logger.LogDebug("Committed initial offset {Offset} for partition {Partition}", initialOffset, partition);
                    }
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            if (_disposed) return ValueTask.CompletedTask;
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
            
            return ValueTask.CompletedTask;
        }
    }
}
