using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks;
using DDD.Domain.Model.BuildingBlocks.Event;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class EventIdConverter : JsonConverter<EventId>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(EventId);
        }
        
        public override EventId Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            new EventId() { Value = reader.GetString() };

        public override void Write(
            Utf8JsonWriter writer,
            EventId eventIdValue,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(eventIdValue.Value);
    }
}