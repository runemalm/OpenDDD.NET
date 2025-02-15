namespace OpenDDD.Infrastructure.Persistence.Storage
{
    public interface IStorage
    {
        Task SaveAsync<T>(string tableName, object id, T entity, CancellationToken ct);
        Task<T?> LoadAsync<T>(string tableName, object id, CancellationToken ct);
        Task<IEnumerable<T>> LoadAllAsync<T>(string tableName, CancellationToken ct);
        Task DeleteAsync(string tableName, object id, CancellationToken ct);
    }
}
