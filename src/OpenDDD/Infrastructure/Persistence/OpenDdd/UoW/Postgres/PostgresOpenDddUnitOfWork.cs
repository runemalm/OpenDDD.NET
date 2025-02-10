using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.TransactionalOutbox;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.UoW.Postgres
{
    public class PostgresOpenDddUnitOfWork : UnitOfWorkBase
    {
        private readonly PostgresDatabaseSession _session;

        public PostgresOpenDddUnitOfWork(
            IDatabaseSession databaseSession,
            IDomainPublisher domainPublisher,
            IIntegrationPublisher integrationPublisher,
            IOutboxRepository outboxRepository,
            ILogger<PostgresOpenDddUnitOfWork> logger)
            : base(domainPublisher, integrationPublisher, outboxRepository, logger)
        {
            _session = databaseSession as PostgresDatabaseSession 
                       ?? throw new InvalidOperationException("Invalid database session type for PostgresOpenDddUnitOfWork. Expected PostgresDatabaseSession.");
        }

        protected override async Task BeginTransactionInternalAsync(CancellationToken ct)
        {
            await _session.BeginTransactionAsync(ct);
        }

        protected override async Task CommitTransactionInternalAsync(CancellationToken ct)
        {
            await _session.CommitTransactionAsync(ct);
        }

        protected override async Task RollbackTransactionInternalAsync(CancellationToken ct)
        {
            await _session.RollbackTransactionAsync(ct);
        }

        protected override void DisposeInternal()
        {
            _session.RollbackTransactionAsync().GetAwaiter().GetResult();
        }
    }
}
