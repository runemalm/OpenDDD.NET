using System.Collections.Concurrent;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events.InMemory
{
    public class InMemoryMessagingProvider : IMessagingProvider
    {
        private readonly OpenDddOptions _openDddOptions;
        private readonly ConcurrentDictionary<string, List<Func<string, Task>>> _subscribers = new();

        public InMemoryMessagingProvider(OpenDddOptions openDddOptions)
        {
            _openDddOptions = openDddOptions;
        }

        public Task SubscribeAsync(string topic, Func<string, Task> messageHandler)
        {
            var consumerGroup = _openDddOptions.EventsListenerGroup;

            var groupKey = $"{topic}:{consumerGroup}";
    
            if (!_subscribers.ContainsKey(groupKey))
            {
                _subscribers[groupKey] = new List<Func<string, Task>>();
            }

            _subscribers[groupKey].Add(messageHandler);
            Console.WriteLine($"Subscribed to topic: {topic} in consumer group: {consumerGroup}");
            return Task.CompletedTask;
        }

        public Task PublishAsync(string topic, string message)
        {
            // Publish to all groups for the topic
            var matchingGroups = _subscribers.Keys.Where(key => key.StartsWith($"{topic}:"));
    
            foreach (var groupKey in matchingGroups)
            {
                if (_subscribers.TryGetValue(groupKey, out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        _ = Task.Run(() => handler(message));
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
