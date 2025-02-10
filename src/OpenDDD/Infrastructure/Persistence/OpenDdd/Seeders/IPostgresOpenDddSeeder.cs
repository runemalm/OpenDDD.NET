using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.Seeders
{
    public interface IPostgresOpenDddSeeder
    {
        Task ExecuteAsync(PostgresDatabaseSession session, CancellationToken ct);
    }
}
