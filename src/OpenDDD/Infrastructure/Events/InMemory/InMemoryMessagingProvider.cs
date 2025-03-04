using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Infrastructure.Events.InMemory
{
    public class InMemoryMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, List<string>> _messageLog = new();
        private readonly ConcurrentDictionary<string, int> _consumerOffsets = new();
        private readonly ConcurrentDictionary<string, ConcurrentBag<Func<string, CancellationToken, Task>>> _subscribers = new();
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

        private static string GetGroupKey(string topic, string consumerGroup)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(consumerGroup))
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));

            return $"{topic}:{consumerGroup}";
        }

        public Task SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken ct)
        {
            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));

            var groupKey = GetGroupKey(topic, consumerGroup);
            var handlers = _subscribers.GetOrAdd(groupKey, _ => new ConcurrentBag<Func<string, CancellationToken, Task>>());

            handlers.Add(messageHandler);
            _logger.LogDebug("Subscribed to topic: {Topic} in listener group: {ConsumerGroup}", topic, consumerGroup);

            if (!_consumerOffsets.ContainsKey(groupKey))
            {
                var messageCount = _messageLog.TryGetValue(topic, out var messages) ? messages.Count : 0;
                _consumerOffsets[groupKey] = messageCount;
                _logger.LogDebug("Consumer group '{ConsumerGroup}' is subscribing for the first time, starting at offset {Offset}.", consumerGroup, messageCount);
                return Task.CompletedTask;
            }

            if (_messageLog.TryGetValue(topic, out var storedMessages))
            {
                var offset = _consumerOffsets[groupKey];
                var unseenMessages = storedMessages.Skip(offset).ToList();

                foreach (var msg in unseenMessages)
                {
                    _ = Task.Run(async () => await messageHandler(msg, ct), ct);
                    _consumerOffsets[groupKey]++;
                }
            }

            return Task.CompletedTask;
        }

        public Task UnsubscribeAsync(string topic, string consumerGroup, CancellationToken cancellationToken = default)
        {
            var groupKey = GetGroupKey(topic, consumerGroup);

            if (_subscribers.TryRemove(groupKey, out _))
            {
                _logger.LogDebug("Unsubscribed from topic: {Topic} in listener group: {ConsumerGroup}", topic, consumerGroup);
            }
            else
            {
                _logger.LogWarning("No active subscriptions found for topic: {Topic} in listener group: {ConsumerGroup}", topic, consumerGroup);
            }

            return Task.CompletedTask;
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

            var matchingGroups = _subscribers.Keys.Where(key => key.StartsWith($"{topic}:")).ToList();

            foreach (var groupKey in matchingGroups)
            {
                if (_subscribers.TryGetValue(groupKey, out var handlers) && handlers.Any())
                {
                    var handler = handlers.OrderBy(_ => Guid.NewGuid()).First();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await handler(message, ct);
                            _consumerOffsets.AddOrUpdate(groupKey, 0, (_, currentOffset) => currentOffset + 1); // Update offset
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error in handler for topic '{topic}': {ex.Message}");
                            _retryQueue.Enqueue((topic, message, groupKey.Split(':')[1], 1));
                        }
                    }, ct);
                }
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

                    var groupKey = GetGroupKey(topic, consumerGroup);
                    if (_subscribers.TryGetValue(groupKey, out var handlers) && handlers.Any())
                    {
                        var handler = handlers.OrderBy(_ => Guid.NewGuid()).First();

                        await Task.Delay(ComputeBackoff(retryCount), _cts.Token); // Exponential backoff

                        try
                        {
                            await handler(message, _cts.Token);
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
