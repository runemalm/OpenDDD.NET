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

        public async Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default) 
            where TDomainEvent : IDomainEvent
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            var topic = EventTopicHelper.DetermineTopic(typeof(TDomainEvent), _options.EventsNamespacePrefix);

            var message = EventSerializer.Serialize(domainEvent);

            Console.WriteLine($"Serialized message: {message}");

            await _messagingProvider.PublishAsync(topic, message, cancellationToken);
        }
    }
}
