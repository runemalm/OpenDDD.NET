using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.Seeders.InMemory
{
    public interface IInMemoryOpenDddSeeder
    {
        Task ExecuteAsync(InMemoryDatabaseSession session, CancellationToken ct);
    }
}
