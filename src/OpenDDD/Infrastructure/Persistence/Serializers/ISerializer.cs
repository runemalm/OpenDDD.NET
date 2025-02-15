namespace OpenDDD.Infrastructure.Persistence.Serializers
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string json);
    }
}
