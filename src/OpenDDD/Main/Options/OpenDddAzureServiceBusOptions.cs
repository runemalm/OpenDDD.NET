namespace OpenDDD.Main.Options
{
    public class OpenDddAzureServiceBusOptions
    {
        public string ConnectionString { get; set; } = "Endpoint=sb://your-servicebus.servicebus.windows.net/;SharedAccessKeyName=your-key;SharedAccessKey=your-key";
        public bool AutoCreateTopics { get; set; } = true;
    }
}
