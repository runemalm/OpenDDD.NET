namespace OpenDDD.API.Options
{
    public class OpenDddEventsOptions
    {
        public string DomainEventTopicTemplate { get; set; } = "YourProjectName.Domain.{EventName}";
        public string IntegrationEventTopicTemplate { get; set; } = "YourProjectName.Interchange.{EventName}";
        public string ListenerGroup { get; set; } = "Default";
    }
}
