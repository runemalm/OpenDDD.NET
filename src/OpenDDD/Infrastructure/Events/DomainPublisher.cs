using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events
{
    public class DomainPublisher : EventPublisherBase, IDomainPublisher
    {
        public DomainPublisher(IMessagingProvider messagingProvider, OpenDddOptions options)
            : base(messagingProvider, options) { }

        public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct) 
            where TEvent : IDomainEvent
        {
            await PublishAsync(domainEvent, "Domain", ct);
        }
    }
}
