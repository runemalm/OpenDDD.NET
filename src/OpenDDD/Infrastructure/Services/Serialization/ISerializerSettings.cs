using Newtonsoft.Json;

namespace OpenDDD.Infrastructure.Services.Serialization
{
    public interface ISerializerSettings
    {
        JsonSerializerSettings JsonSerializerSettings { get; }
    }
}
