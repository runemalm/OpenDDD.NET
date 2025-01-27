namespace OpenDDD.Infrastructure.TransactionalOutbox
{
    public class OutboxEntry
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
