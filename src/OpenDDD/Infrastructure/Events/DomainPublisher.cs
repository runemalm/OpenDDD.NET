using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Events
{
    public class DomainPublisher : IDomainPublisher
    {
        private readonly List<IDomainEvent> _publishedEvents = new();

        public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct) 
            where TEvent : IDomainEvent
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            _publishedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }

        public IReadOnlyList<IDomainEvent> GetPublishedEvents() => _publishedEvents.AsReadOnly();
    }
}
