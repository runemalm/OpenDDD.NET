using OpenDDD.Domain.Model;

namespace OpenDDD.Tests.Infrastructure.Events
{
    public class TestEvent : IDomainEvent
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public DateTime OccurredAt { get; set; }

        public TestEvent() { }

        public TestEvent(Guid eventId, string eventName, DateTime occurredAt)
        {
            EventId = eventId;
            EventName = eventName;
            OccurredAt = occurredAt;
        }
    }
}
