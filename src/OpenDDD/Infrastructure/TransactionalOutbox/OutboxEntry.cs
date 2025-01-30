namespace OpenDDD.Infrastructure.TransactionalOutbox
{
    public class OutboxEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventType { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string Payload { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public bool Processed { get; set; } = false;
    }
}
