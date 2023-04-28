using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Aggregate;
using OpenDDD.Domain.Model.BuildingBlocks.Entity;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;

namespace OpenDDD.Infrastructure.Ports.Repository
{
    public abstract class Repository<T> : IRepository<T> where T : IAggregate
    {
        public abstract void Start(CancellationToken ct);
        public abstract Task StartAsync(CancellationToken ct);
        public abstract void Stop(CancellationToken ct);
        public abstract Task StopAsync(CancellationToken ct);
        public abstract void DeleteAll(ActionId actionId, CancellationToken ct);
        public abstract Task DeleteAllAsync(ActionId actionId, CancellationToken ct);
        public abstract Task DeleteAsync(EntityId entityId, ActionId actionId, CancellationToken ct);
        public abstract IEnumerable<T> GetAll(ActionId actionId, CancellationToken ct);
        public abstract Task<IEnumerable<T>> GetAllAsync(ActionId actionId, CancellationToken ct);
        public abstract Task<T> GetAsync(EntityId entityId, ActionId actionId, CancellationToken ct);
        public abstract Task<IEnumerable<T>> GetAsync(IEnumerable<EntityId> entityIds, ActionId actionId, CancellationToken ct);
        public abstract T GetFirstOrDefaultWith(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
        public abstract Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
        public abstract T GetFirstOrDefaultWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        public abstract Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        public abstract IEnumerable<T> GetWith(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
        public abstract Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
        public abstract IEnumerable<T> GetWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        public abstract Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
        public abstract void Save(T aggregate, ActionId actionId, CancellationToken ct);
        public abstract Task SaveAsync(T aggregate, ActionId actionId, CancellationToken ct);
        public abstract string GetNextIdentity();
        public abstract Task<string> GetNextIdentityAsync();
        
        protected string FormatPropertyName(string name, SerializerSettings serializerSettings)
        {
            /*
             * Format the property name according to the
             * jsonserializersettings naming strategy of the contract resolver.
             *
             * Found no way to get the naming strategy, hence the hacked solution.
             */
            var formatted = "";

            var dummyDict = 
                new Dictionary<string, string>
                {
                    { name, "dummyValue" }
                };
            var dummyJson = JsonConvert.SerializeObject(dummyDict, serializerSettings);
            var dummyObject = JObject.FromObject(JsonConvert.DeserializeObject(dummyJson, serializerSettings));

            foreach (var kvp in dummyObject)
                formatted = kvp.Key;

            return formatted;
        }
    }
}
