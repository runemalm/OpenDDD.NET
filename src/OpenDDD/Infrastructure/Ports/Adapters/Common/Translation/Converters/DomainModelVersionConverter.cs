﻿using System;
using Newtonsoft.Json;
using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class DomainModelVersionConverter : Converter<DomainModelVersion>
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
            string s = (string)reader.Value;
            return new DomainModelVersion(s);
        }
    }
}