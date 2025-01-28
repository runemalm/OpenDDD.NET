using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events.InMemory
{
    public class InMemoryMessagingProvider : IMessagingProvider
    {
        private readonly OpenDddOptions _openDddOptions;
        private readonly ConcurrentDictionary<string, ConcurrentBag<Func<string, CancellationToken, Task>>> _subscribers = new();
        private readonly ILogger<InMemoryMessagingProvider> _logger;

        public InMemoryMessagingProvider(OpenDddOptions openDddOptions, ILogger<InMemoryMessagingProvider> logger)
        {
            _openDddOptions = openDddOptions ?? throw new ArgumentNullException(nameof(openDddOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SubscribeAsync(string topic, Func<string, CancellationToken, Task> messageHandler, CancellationToken ct)
        {
            var consumerGroup = _openDddOptions.EventsListenerGroup;

            var groupKey = $"{topic}:{consumerGroup}";

            var handlers = _subscribers.GetOrAdd(groupKey, _ => new ConcurrentBag<Func<string, CancellationToken, Task>>());

            handlers.Add(messageHandler);

            _logger.LogInformation("Subscribed to topic: {Topic} in listener group: {ConsumerGroup}", topic, consumerGroup);
            return Task.CompletedTask;
        }

        public Task PublishAsync(string topic, string message, CancellationToken ct)
        {
            var matchingGroups = _subscribers.Keys.Where(key => key.StartsWith($"{topic}:"));

            foreach (var groupKey in matchingGroups)
            {
                if (_subscribers.TryGetValue(groupKey, out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await handler(message, ct);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error in handler for topic '{topic}': {ex.Message}");
                            }
                        }, ct);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
