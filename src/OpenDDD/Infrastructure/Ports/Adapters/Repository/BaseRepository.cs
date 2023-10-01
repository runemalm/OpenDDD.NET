using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model.AggregateRoot;
using OpenDDD.Domain.Model.Entity;
using OpenDDD.Infrastructure.Ports.Database;
using OpenDDD.Infrastructure.Ports.Repository;

namespace OpenDDD.Infrastructure.Ports.Adapters.Repository
{
    public abstract class BaseRepository<TEntity, TEntityId, TDatabaseConnection> : IRepository<TEntity, TEntityId, TDatabaseConnection> 
        where TEntity : class, IAggregateRoot, new()
        where TEntityId : EntityId
        where TDatabaseConnection : IDatabaseConnection, IDocumentDatabaseConnection
    {
        public TDatabaseConnection DatabaseConnection { get; set; }
        private string _entityName = GetEntityName(typeof(TEntity));
        
        public void DeleteAll(ActionId actionId, CancellationToken ct)
        {
            DatabaseConnection.TruncateDatabase();
        }

        public async Task DeleteAllAsync(ActionId actionId, CancellationToken ct)
        {
            await DatabaseConnection.TruncateDatabaseAsync();
        }

        // public abstract Task DeleteAsync(TId entityId, ActionId actionId, CancellationToken ct);
        public IEnumerable<TEntity> GetAll(ActionId actionId, CancellationToken ct)
        {
            return DatabaseConnection.GetAll<TEntity>(_entityName);
        }
        public Task<IEnumerable<TEntity>> GetAllAsync(ActionId actionId, CancellationToken ct)
        {
            return DatabaseConnection.GetAllAsync<TEntity>(_entityName);
        }
        // public abstract TEntity Get(TId entityId, ActionId actionId, CancellationToken ct);

        public Task<TEntity> GetAsync(TEntityId entityId, ActionId actionId, CancellationToken ct)
            => GetFirstOrDefaultWithAsync(e => e.Id == entityId, actionId, ct);
        
        // public abstract Task<IEnumerable<TEntity>> GetAsync(IEnumerable<TId> entityIds, ActionId actionId, CancellationToken ct);
        // public abstract TEntity GetFirstOrDefaultWith(Expression<Func<TEntity, bool>> where, ActionId actionId, CancellationToken ct);

        public async Task<TEntity> GetFirstOrDefaultWithAsync(Expression<Func<TEntity, bool>> where, ActionId actionId, CancellationToken ct)
            => (await DatabaseConnection.FindAsync(_entityName, where)).FirstOrDefault();

        // public abstract TEntity GetFirstOrDefaultWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        // public abstract Task<TEntity> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        // public abstract IEnumerable<TEntity> GetWith(Expression<Func<TEntity, bool>> where, ActionId actionId, CancellationToken ct);
        // public abstract Task<IEnumerable<TEntity>> GetWithAsync(Expression<Func<TEntity, bool>> where, ActionId actionId, CancellationToken ct);
        // public abstract IEnumerable<TEntity> GetWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        // public abstract Task<IEnumerable<TEntity>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        public void Save(TEntity aggregate, ActionId actionId, CancellationToken ct)
        {
            DatabaseConnection.UpsertDocument(_entityName, aggregate.Id.ToString(), aggregate);
        }

        public async Task SaveAsync(TEntity aggregate, ActionId actionId, CancellationToken ct)
        {
            await DatabaseConnection.UpsertDocumentAsync(_entityName, aggregate.Id.ToString(), aggregate);
        }
        // public abstract string GetNextIdentity();
        // public abstract Task<string> GetNextIdentityAsync();
        
        public BaseRepository(TDatabaseConnection databaseConnection)
        {
            DatabaseConnection = databaseConnection;
        }

        // Private
        
        private static string GetEntityName(Type type)
        {
            // Get the full type name as a string
            string fullName = type.FullName;

            // Extract the class name (excluding namespace and generic type parameters)
            int index = fullName.LastIndexOf('.');
            if (index >= 0)
            {
                return fullName.Substring(index + 1).ToLower();
            }
            return fullName.ToLower();
        }
    }
}
