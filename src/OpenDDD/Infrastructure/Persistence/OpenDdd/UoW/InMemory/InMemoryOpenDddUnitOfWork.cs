using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.TransactionalOutbox;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory;
using OpenDDD.Infrastructure.Persistence.UoW;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.UoW.InMemory
{
    public class InMemoryOpenDddUnitOfWork : UnitOfWorkBase
    {
        private readonly InMemoryDatabaseSession _session;

        public InMemoryOpenDddUnitOfWork(
            IDatabaseSession databaseSession,
            IDomainPublisher domainPublisher,
            IIntegrationPublisher integrationPublisher,
            IOutboxRepository outboxRepository,
            ILogger<InMemoryOpenDddUnitOfWork> logger)
            : base(domainPublisher, integrationPublisher, outboxRepository, logger)
        {
            _session = databaseSession as InMemoryDatabaseSession 
                       ?? throw new InvalidOperationException("Invalid database session type for InMemoryOpenDddUnitOfWork.");
        }

        protected override Task BeginTransactionInternalAsync(CancellationToken ct) => _session.BeginTransactionAsync(ct);
        protected override Task CommitTransactionInternalAsync(CancellationToken ct) => _session.CommitTransactionAsync(ct);
        protected override Task RollbackTransactionInternalAsync(CancellationToken ct) => _session.RollbackTransactionAsync(ct);
        protected override void DisposeInternal() => _session.RollbackTransactionAsync().GetAwaiter().GetResult();
    }
}
