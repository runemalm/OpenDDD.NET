namespace OpenDDD.Domain.Model.Helpers
{
    public static class EventTopicHelper
    {
        public static string DetermineTopic(Type eventType, string namespacePrefix)
        {
            var suffix = eventType.Name.EndsWith("DomainEvent") ? "DomainEvent" : "IntegrationEvent";
            var topicType = suffix == "DomainEvent" ? "Domain" : "Interchange";
            var eventName = eventType.Name.Replace(suffix, "");
            return $"{namespacePrefix}.{topicType}.{eventName}";
        }
    }
}
