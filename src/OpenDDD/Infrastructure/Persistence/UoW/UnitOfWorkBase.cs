using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.TransactionalOutbox;

namespace OpenDDD.Infrastructure.Persistence.UoW
{
    public abstract class UnitOfWorkBase : IUnitOfWork
    {
        private readonly IDomainPublisher _domainPublisher;
        private readonly IIntegrationPublisher _integrationPublisher;
        private readonly IOutboxRepository _outboxRepository;
        protected readonly ILogger<UnitOfWorkBase> _logger;

        private bool _disposed = false;
        
        protected UnitOfWorkBase(
            IDomainPublisher domainPublisher, 
            IIntegrationPublisher integrationPublisher, 
            IOutboxRepository outboxRepository, 
            ILogger<UnitOfWorkBase> logger)
        {
            _domainPublisher = domainPublisher ?? throw new ArgumentNullException(nameof(domainPublisher));
            _integrationPublisher = integrationPublisher ?? throw new ArgumentNullException(nameof(integrationPublisher));
            _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await BeginTransactionInternalAsync(ct);
        }

        public async Task AddEventsToOutboxAsync(CancellationToken ct)
        {
            foreach (var domainEvent in _domainPublisher.GetPublishedEvents())
            {
                _logger.LogDebug("Saving domain event to outbox: {EventType}", domainEvent.GetType().Name);
                await _outboxRepository.SaveEventAsync(domainEvent, ct);
            }

            foreach (var integrationEvent in _integrationPublisher.GetPublishedEvents())
            {
                _logger.LogDebug("Saving integration event to outbox: {EventType}", integrationEvent.GetType().Name);
                await _outboxRepository.SaveEventAsync(integrationEvent, ct);
            }
        }

        public async Task CommitAsync(CancellationToken ct)
        {
            try
            {
                await AddEventsToOutboxAsync(ct);
                await CommitTransactionInternalAsync(ct);
            }
            catch
            {
                await RollbackAsync(ct);
                throw;
            }
        }

        public async Task RollbackAsync(CancellationToken ct)
        {
            await RollbackTransactionInternalAsync(ct);
        }

        protected abstract Task BeginTransactionInternalAsync(CancellationToken ct);
        protected abstract Task CommitTransactionInternalAsync(CancellationToken ct);
        protected abstract Task RollbackTransactionInternalAsync(CancellationToken ct);

        public void Dispose()
        {
            if (!_disposed)
            {
                DisposeInternal();
                _disposed = true;
            }
        }

        protected abstract void DisposeInternal();
    }
}
