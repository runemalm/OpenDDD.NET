namespace OpenDDD.API.Options
{
    public class OpenDddEventsOptions
    {
        public string DomainEventTopic { get; set; } = "YourProjectName.Domain.{EventName}";
        public string IntegrationEventTopic { get; set; } = "YourProjectName.Interchange.{EventName}";
        public string ListenerGroup { get; set; } = "Default";
    }
}
