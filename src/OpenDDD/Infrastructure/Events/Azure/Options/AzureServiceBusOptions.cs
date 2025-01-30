namespace OpenDDD.Infrastructure.Events.Azure.Options
{
    public class AzureServiceBusOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public bool AutoCreateTopics { get; set; } = true;
    }
}
