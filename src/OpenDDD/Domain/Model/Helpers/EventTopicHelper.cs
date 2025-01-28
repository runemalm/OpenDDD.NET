namespace OpenDDD.Domain.Model.Helpers
{
    public static class EventTopicHelper
    {
        public static string DetermineTopic(Type eventClassType, string namespacePrefix)
        {
            if (eventClassType == null) throw new ArgumentNullException(nameof(eventClassType));

            try
            {
                var eventType = eventClassType.Name.EndsWith("IntegrationEvent") ? "Integration" : "Domain";
                var contextType = eventType == "Domain" ? "Domain" : "Interchange";
                var eventName = eventClassType.Name.Replace("IntegrationEvent", "");

                if (string.IsNullOrWhiteSpace(namespacePrefix))
                {
                    throw new InvalidOperationException("Namespace prefix cannot be null or empty.");
                }

                return $"{namespacePrefix}.{contextType}.{eventName}";
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error determining topic for event type {eventClassType?.FullName}: {ex.Message}");
                throw;
            }
        }
    }
}
