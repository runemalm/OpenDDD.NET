using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events
{
    public class IntegrationPublisher : EventPublisherBase, IIntegrationPublisher
    {
        public IntegrationPublisher(IMessagingProvider messagingProvider, OpenDddOptions options)
            : base(messagingProvider, options) { }

        public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct) 
            where TEvent : IIntegrationEvent
        {
            await PublishAsync(integrationEvent, "Interchange", ct);
        }
    }
}
