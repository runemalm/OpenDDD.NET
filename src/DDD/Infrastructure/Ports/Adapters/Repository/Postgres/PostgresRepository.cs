using DDD.Application.Settings;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;
using DDD.Infrastructure.Ports.Adapters.Repository.Ado;
using DDD.Infrastructure.Ports.Repository;
using DDD.Infrastructure.Services.Persistence;

namespace DDD.Infrastructure.Ports.Adapters.Repository.Postgres
{
	public class PostgresRepository<T, TId> : AdoRepository<T, TId> where T : IAggregate where TId : EntityId
	{
		public PostgresRepository(ISettings settings, string tableName, IMigrator<T> migrator,
			IPersistenceService persistenceService) :
			base(settings, tableName, migrator, persistenceService)
		{
			
		}
	}
}
