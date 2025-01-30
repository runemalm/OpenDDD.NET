using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Events
{
    public class IntegrationPublisher : IIntegrationPublisher
    {
        private readonly List<IIntegrationEvent> _publishedEvents = new();

        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct) 
            where TEvent : IIntegrationEvent
        {
            if (integrationEvent == null) throw new ArgumentNullException(nameof(integrationEvent));

            _publishedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }

        public IReadOnlyList<IIntegrationEvent> GetPublishedEvents() => _publishedEvents.AsReadOnly();
    }
}
