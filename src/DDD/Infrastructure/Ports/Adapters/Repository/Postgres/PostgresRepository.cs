using DDD.Application.Settings;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Adapters.Repository.Sql;
using DDD.Infrastructure.Ports.Repository;
using DDD.Infrastructure.Services.Persistence;

namespace DDD.Infrastructure.Ports.Adapters.Repository.Postgres
{
	public class PostgresRepository<T, TId> : SqlRepository<T, TId> where T : IAggregate where TId : EntityId
	{
		public PostgresRepository(ISettings settings, string tableName, IMigrator<T> migrator,
			IPersistenceService persistenceService, SerializerSettings serializerSettings) :
			base(settings, tableName, migrator, persistenceService, serializerSettings)
		{
			
		}
	}
}
