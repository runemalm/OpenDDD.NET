using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Events.Base;

namespace OpenDDD.Infrastructure.Events.InMemory
{
    public class InMemoryMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, List<string>> _messageLog = new();
        private readonly ConcurrentDictionary<string, int> _consumerGroupOffsets = new();
        private readonly ConcurrentDictionary<string, InMemorySubscription> _subscriptions = new();
        private readonly ConcurrentQueue<(string Topic, string Message, string ConsumerGroup, int RetryCount)> _retryQueue = new();
        private readonly ILogger<InMemoryMessagingProvider> _logger;
        private readonly TimeSpan _initialRetryDelay = TimeSpan.FromSeconds(1);
        private readonly Task _retryTask;
        private readonly int _maxRetries = 5;
        private readonly CancellationTokenSource _cts = new();

        public InMemoryMessagingProvider(ILogger<InMemoryMessagingProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryTask = Task.Run(ProcessRetries, _cts.Token);  // Start retry processing loop
        }

        public async Task<ISubscription> SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken ct = default)
        {
            if (messageHandler is null)
                throw new ArgumentException(nameof(messageHandler));
            
            var subscription = new InMemorySubscription(topic, consumerGroup, messageHandler);
            _subscriptions[subscription.Id] = subscription;

            _logger.LogDebug("Subscribed to topic: {Topic} in listener group: {ConsumerGroup}, Subscription ID: {SubscriptionId}", 
                topic, consumerGroup, subscription.Id);

            var groupKey = $"{topic}:{consumerGroup}";

            if (!_consumerGroupOffsets.ContainsKey(groupKey))
            {
                _consumerGroupOffsets[groupKey] = _messageLog.TryGetValue(topic, out var messages) ? messages.Count : 0;
                _logger.LogDebug("First subscription in consumer group '{ConsumerGroup}', starting at offset {Offset}.", 
                    consumerGroup, _consumerGroupOffsets[groupKey]);
            }
            else
            {
                var offset = _consumerGroupOffsets[groupKey];
                if (_messageLog.TryGetValue(topic, out var storedMessages))
                {
                    var unseenMessages = storedMessages.Skip(offset).ToList();
                    foreach (var msg in unseenMessages)
                    {
                        _ = Task.Run(async () => await messageHandler(msg, ct), ct);
                        _consumerGroupOffsets[groupKey]++;
                    }
                }
            }

            return subscription;
        }

        public async Task UnsubscribeAsync(ISubscription subscription, CancellationToken cancellationToken = default)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            if (!_subscriptions.TryRemove(subscription.Id, out _))
            {
                _logger.LogWarning("No active subscription found with ID {SubscriptionId}", subscription.Id);
                return;
            }

            _logger.LogDebug("Unsubscribed from topic: {Topic} in listener group: {ConsumerGroup}, Subscription ID: {SubscriptionId}", 
                subscription.Topic, subscription.ConsumerGroup, subscription.Id);

            await subscription.DisposeAsync();
        }

        public Task PublishAsync(string topic, string message, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            var messages = _messageLog.GetOrAdd(topic, _ => new List<string>());
            lock (messages)
            {
                messages.Add(message);
            }

            var groupSubscriptions = _subscriptions.Values
                .Where(s => s.Topic == topic)
                .GroupBy(s => s.ConsumerGroup);

            foreach (var group in groupSubscriptions)
            {
                var subscription = group.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                if (subscription == null) continue;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await subscription.MessageHandler(message, ct);
                        _consumerGroupOffsets.AddOrUpdate($"{topic}:{subscription.ConsumerGroup}", 0, (_, currentOffset) => currentOffset + 1);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error in handler for topic '{topic}' in consumer group '{subscription.ConsumerGroup}': {ex.Message}");
                        _retryQueue.Enqueue((topic, message, subscription.ConsumerGroup, 1));
                    }
                }, ct);
            }

            return Task.CompletedTask;
        }

        private async Task ProcessRetries()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (_retryQueue.TryDequeue(out var retryMessage))
                {
                    var (topic, message, consumerGroup, retryCount) = retryMessage;

                    if (retryCount > _maxRetries)
                    {
                        _logger.LogError("Message dropped after exceeding max retries: {Message}", message);
                        continue;
                    }

                    var subscription = _subscriptions.Values.FirstOrDefault(s => s.Topic == topic && s.ConsumerGroup == consumerGroup);
                    if (subscription != null)
                    {
                        await Task.Delay(ComputeBackoff(retryCount), _cts.Token);

                        try
                        {
                            await subscription.MessageHandler(message, _cts.Token);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Retry failed for message: {message}");
                            _retryQueue.Enqueue((topic, message, consumerGroup, retryCount + 1));
                        }
                    }
                }
                else
                {
                    await Task.Delay(500, _cts.Token);
                }
            }
        }

        private TimeSpan ComputeBackoff(int retryCount)
        {
            return TimeSpan.FromMilliseconds(_initialRetryDelay.TotalMilliseconds * Math.Pow(2, retryCount));
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            
            foreach (var subscription in _subscriptions.Values)
            {
                await subscription.DisposeAsync();
            }

            _subscriptions.Clear();

            try
            {
                await _retryTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Retry processing task canceled.");
            }
        }
    }
}
