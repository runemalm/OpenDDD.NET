using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Helpers;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events.Base
{
    public abstract class EventPublisherBase
    {
        private readonly IMessagingProvider _messagingProvider;
        private readonly OpenDddOptions _options;

        protected EventPublisherBase(IMessagingProvider messagingProvider, OpenDddOptions options)
        {
            _messagingProvider = messagingProvider ?? throw new ArgumentNullException(nameof(messagingProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected async Task PublishAsync<TEvent>(TEvent @event, string contextType, CancellationToken ct)
            where TEvent : IEvent
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var topic = EventTopicHelper.DetermineTopic(typeof(TEvent), _options.EventsNamespacePrefix, contextType);
            var message = EventSerializer.Serialize(@event);
            try
            {
                await _messagingProvider.PublishAsync(topic, message, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing event of type {typeof(TEvent).Name} to topic {topic}: {ex.Message}");
                throw;
            }
        }
    }
}
