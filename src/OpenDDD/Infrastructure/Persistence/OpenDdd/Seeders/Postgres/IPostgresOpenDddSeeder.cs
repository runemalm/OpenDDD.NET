using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.Seeders.Postgres
{
    public interface IPostgresOpenDddSeeder
    {
        Task ExecuteAsync(PostgresDatabaseSession session, CancellationToken ct);
    }
}
