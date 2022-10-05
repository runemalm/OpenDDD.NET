using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DDD.Application;
using DDD.Domain;
using DDD.Domain.Model;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class ActionIdConverter : JsonConverter<ActionId>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(ActionId);
        }
        
        public override ActionId Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            new ActionId() { Value = reader.GetString() };

        public override void Write(
            Utf8JsonWriter writer,
            ActionId actionIdValue,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(actionIdValue.Value);
    }
}