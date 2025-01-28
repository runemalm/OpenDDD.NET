using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Helpers;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events
{
    public class IntegrationPublisher : IIntegrationPublisher
    {
        private readonly IMessagingProvider _messagingProvider;
        private readonly OpenDddOptions _options;

        public IntegrationPublisher(IMessagingProvider messagingProvider, OpenDddOptions options)
        {
            _messagingProvider = messagingProvider ?? throw new ArgumentNullException(nameof(messagingProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct) 
            where TEvent : IIntegrationEvent
        {
            if (integrationEvent == null) throw new ArgumentNullException(nameof(integrationEvent));

            var topic = EventTopicHelper.DetermineTopic(typeof(TEvent), _options.EventsNamespacePrefix);
            var message = EventSerializer.Serialize(integrationEvent);
            await _messagingProvider.PublishAsync(topic, message, ct);
        }
    }
}
