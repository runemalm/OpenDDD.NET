using System.Text.Json;
using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Events
{
    public static class EventSerializer
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never // Ensure all properties are serialized
        };

        public static string Serialize<TEvent>(TEvent domainEvent) where TEvent : IEvent
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
            return JsonSerializer.Serialize(domainEvent, SerializerOptions);
        }

        public static TEvent Deserialize<TEvent>(string message) where TEvent : IEvent
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message));
            return JsonSerializer.Deserialize<TEvent>(message, SerializerOptions) 
                   ?? throw new InvalidOperationException($"Failed to deserialize message to type {typeof(TEvent).Name}");
        }
    }
}
