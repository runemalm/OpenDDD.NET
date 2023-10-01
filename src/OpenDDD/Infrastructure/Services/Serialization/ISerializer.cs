namespace OpenDDD.Infrastructure.Services.Serialization
{
    public interface ISerializer : IInfrastructureService
    {
        ISerializerSettings Settings { get; set; }

        string Serialize<T>(T input);
        T Deserialize<T>(string input);
        object Deserialize(string input);
    }
}
