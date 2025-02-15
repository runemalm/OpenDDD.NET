using Microsoft.Extensions.Logging;
using OpenDDD.API.Options;

namespace OpenDDD.Domain.Model.Helpers
{
    public static class EventTopicHelper
    {
        public static string DetermineTopic(Type eventClassType, OpenDddEventsOptions eventOptions, ILogger logger)
        {
            if (eventClassType == null) throw new ArgumentNullException(nameof(eventClassType));
            if (eventOptions == null) throw new ArgumentNullException(nameof(eventOptions));

            try
            {
                // Determine if it's a domain event or integration event
                bool isIntegrationEvent = eventClassType.Name.EndsWith("IntegrationEvent");
                string eventName = eventClassType.Name.Replace("IntegrationEvent", "");

                // Select the correct format from configuration
                string topicTemplate = isIntegrationEvent 
                    ? eventOptions.IntegrationEventTopicTemplate 
                    : eventOptions.DomainEventTopicTemplate;

                // Ensure the topic format contains "{EventName}"
                if (!topicTemplate.Contains("{EventName}"))
                {
                    throw new InvalidOperationException("Event topic format must contain '{EventName}' placeholder.");
                }

                // Replace placeholder with actual event name
                return topicTemplate.Replace("{EventName}", eventName);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error determining topic for event type {EventType}.", eventClassType.FullName);
                throw;
            }
        }
        
        public static string DetermineTopic(string eventType, string eventName, OpenDddEventsOptions eventOptions, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentNullException(nameof(eventType));
            if (string.IsNullOrWhiteSpace(eventName)) throw new ArgumentNullException(nameof(eventName));
            if (eventOptions == null) throw new ArgumentNullException(nameof(eventOptions));

            try
            {
                // Ensure eventType is either "Integration" or "Domain"
                if (eventType != "Integration" && eventType != "Domain")
                {
                    throw new ArgumentException($"Invalid event type: {eventType}. Expected 'Integration' or 'Domain'.", nameof(eventType));
                }

                // Select the correct format from configuration
                string topicTemplate = eventType == "Integration"
                    ? eventOptions.IntegrationEventTopicTemplate
                    : eventOptions.DomainEventTopicTemplate;

                // Ensure the topic format contains "{EventName}"
                if (!topicTemplate.Contains("{EventName}"))
                {
                    throw new InvalidOperationException("Event topic format must contain '{EventName}' placeholder.");
                }

                // Replace placeholder with actual event name
                return topicTemplate.Replace("{EventName}", eventName);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error determining topic for event type {EventType} and event name {EventName}.", eventType, eventName);
                throw;
            }
        }
    }
}
