// using Newtonsoft.Json;
// using OpenDDD.Domain.Model.BuildingBlocks;
// using OpenDDD.Infrastructure.Services.Serialization;
//
// namespace OpenDDD.NET
// {
//     public interface ISerializing<T> where T : IBuildingBlock
//     {
//         ISerializerSettings SerializerSettings { get; set; }
//
//         // Default implementation for serializing object of type T
//         string Serialize(T input)
//         {
//             string serialized = JsonConvert.SerializeObject(input, (JsonSerializerSettings)SerializerSettings);
//             return serialized;
//         }
//
//         // Default implementation for deserializing string into object of type T
//         T Deserialize(string input)
//         {
//             T deserialized = JsonConvert.DeserializeObject<T>(input, (JsonSerializerSettings)SerializerSettings);
//             return deserialized;
//         }
//     }
// }
