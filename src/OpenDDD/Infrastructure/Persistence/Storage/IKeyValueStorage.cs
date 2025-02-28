namespace OpenDDD.Infrastructure.Persistence.Storage
{
    public interface IKeyValueStorage
    {
        Task PutAsync<T>(string key, T value, CancellationToken ct);
        Task<T?> GetAsync<T>(string key, CancellationToken ct);
        Task<IEnumerable<T>> GetByPrefixAsync<T>(string keyPrefix, CancellationToken ct);
        Task RemoveAsync(string key, CancellationToken ct);
        Task ClearAsync(CancellationToken ct);
    }
}
