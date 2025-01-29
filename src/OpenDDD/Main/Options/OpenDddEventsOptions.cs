namespace OpenDDD.Main.Options
{
    public class OpenDddEventsOptions
    {
        public string DomainEventTopic { get; set; } = "YourProjectName.Domain.";
        public string IntegrationEventTopic { get; set; } = "YourProjectName.Interchange.";
        public string ListenerGroup { get; set; } = "Default";
    }
}
