using System.Data;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using Npgsql;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres
{
    public class PostgresDatabaseSession : IDatabaseSession
    {
        public NpgsqlConnection Connection { get; }
        public NpgsqlTransaction? Transaction { get; private set; }

        public PostgresDatabaseSession(NpgsqlConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task OpenConnectionAsync(CancellationToken ct = default)
        {
            if (Connection.State == ConnectionState.Closed)
                await Connection.OpenAsync(ct);
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            await OpenConnectionAsync(ct);
            Transaction = await Connection.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (Transaction == null)
                throw new InvalidOperationException("No transaction in progress.");

            await Transaction.CommitAsync(ct);
            await Transaction.DisposeAsync();
            Transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (Transaction != null)
            {
                await Transaction.RollbackAsync(ct);
                await Transaction.DisposeAsync();
                Transaction = null;
            }
        }
    }
}