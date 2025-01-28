using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Helpers;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events
{
    public class DomainPublisher : IDomainPublisher
    {
        private readonly IMessagingProvider _messagingProvider;
        private readonly OpenDddOptions _options;

        public DomainPublisher(IMessagingProvider messagingProvider, OpenDddOptions options)
        {
            _messagingProvider = messagingProvider ?? throw new ArgumentNullException(nameof(messagingProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct) 
            where TEvent : IDomainEvent
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            var topic = EventTopicHelper.DetermineTopic(typeof(TEvent), _options.EventsNamespacePrefix);
            var message = EventSerializer.Serialize(domainEvent);
            await _messagingProvider.PublishAsync(topic, message, ct);
        }
    }
}
