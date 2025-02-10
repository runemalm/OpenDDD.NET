using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.EfCore.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.TransactionalOutbox;

namespace OpenDDD.Infrastructure.Persistence.EfCore.UoW
{
    public class EfCoreUnitOfWork : UnitOfWorkBase
    {
        public readonly EfCoreDatabaseSession Session;
        private readonly bool _isInMemoryDatabase;

        public EfCoreUnitOfWork(
            IDatabaseSession databaseSession,
            IDomainPublisher domainPublisher,
            IIntegrationPublisher integrationPublisher,
            IOutboxRepository outboxRepository,
            ILogger<EfCoreUnitOfWork> logger)
            : base(domainPublisher, integrationPublisher, outboxRepository, logger)
        {
            Session = databaseSession as EfCoreDatabaseSession 
                ?? throw new InvalidOperationException("Invalid database session type for EfCoreUnitOfWork. Expected EfCoreDatabaseSession.");
            
            _isInMemoryDatabase = Session.DbContext.Database.IsInMemory();
        }

        protected override async Task BeginTransactionInternalAsync(CancellationToken ct)
        {
            if (!_isInMemoryDatabase)
            {
                await Session.BeginTransactionAsync(ct);
            }
        }

        protected override async Task CommitTransactionInternalAsync(CancellationToken ct)
        {
            try
            {
                if (!_isInMemoryDatabase)
                {
                    await Session.CommitTransactionAsync(ct);
                }
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        protected override async Task RollbackTransactionInternalAsync(CancellationToken ct)
        {
            if (!_isInMemoryDatabase)
            {
                await Session.RollbackTransactionAsync(ct);
            }
        }

        protected override void DisposeInternal()
        {
            
        }

        private async Task DisposeTransactionAsync()
        {
            if (!_isInMemoryDatabase)
            {
                await Session.RollbackTransactionAsync();
            }
        }
    }
}
