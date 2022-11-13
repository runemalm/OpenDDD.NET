using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;

namespace DDD.Infrastructure.Ports.Repository
{
    public abstract class Repository<T> : IRepository<T> where T : IAggregate
    {
        public abstract Task DeleteAllAsync(ActionId actionId, CancellationToken ct);
        public abstract Task DeleteAsync(EntityId entityId, ActionId actionId, CancellationToken ct);
        public abstract Task<IEnumerable<T>> GetAllAsync(ActionId actionId, CancellationToken ct);
        public abstract Task<T> GetAsync(EntityId entityId, ActionId actionId, CancellationToken ct);
        public abstract Task<IEnumerable<T>> GetAsync(IEnumerable<EntityId> entityIds, ActionId actionId, CancellationToken ct);
        public abstract Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
        public abstract Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        public abstract Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
        public abstract Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        public abstract Task SaveAsync(T aggregate, ActionId actionId, CancellationToken ct);
        public abstract Task<string> GetNextIdentityAsync();
        
        protected string FormatPropertyName(string name)
        {
            /*
             * Format the property name according to JsonSerializer property name
             * camel case setting.
             */
            var opts = ((JsonSerializerOptions)typeof(JsonSerializerOptions)
                .GetField("s_defaultOptions",
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(null));
			
            var isCamelCase = opts.PropertyNamingPolicy == JsonNamingPolicy.CamelCase;
			
            if (isCamelCase)
                name = name.First().ToString().ToLower() + name.Substring(1);
            else
                name = name.First().ToString().ToUpper() + name.Substring(1);
			
            return name;
        }
    }
}
