using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Main.Interfaces;

namespace OpenDDD.Infrastructure.Repository.Postgres.Base
{
    public abstract class PostgresRepositoryBase<TAggregateRoot, TId> :
        IRepository<TAggregateRoot, TId>,
        IStartable, IStoppable
        where TAggregateRoot : AggregateRootBase<TId>
        where TId : notnull
    {
        private readonly ILogger _logger;

        protected PostgresRepositoryBase(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TAggregateRoot> GetAsync(TId id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot?> FindAsync(TId id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TAggregateRoot>> FindWithAsync(Expression<Func<TAggregateRoot, bool>> filterExpression, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting repository of type {GetType().Name}.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping repository of type {GetType().Name}.");
            return Task.CompletedTask;
        }
    }
}
