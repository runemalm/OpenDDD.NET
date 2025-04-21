namespace OpenDDD.Infrastructure.TransactionalOutbox.Options
{
    public class OpenDddOutboxProcessorOptions
    {
        public int PollingIntervalSeconds { get; set; } = 3;
        public int? MaxEventsPerCycle { get; set; } = null;
    }
}
