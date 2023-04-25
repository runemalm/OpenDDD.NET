﻿using Newtonsoft.Json;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using Domain.Model.Notification;

namespace Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class EmailConverter : Converter<Email>
    {
        public override void WriteJson(
            JsonWriter writer, 
            object? value,
            JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
        
        public override object ReadJson(
            JsonReader reader, 
            Type objectType, 
            object? existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;
            return ReadJsonUsingMethod(reader, "Create", objectType);
        }
    }
}
