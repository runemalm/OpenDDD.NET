using DDD.Domain;
using DDD.Infrastructure.Persistence;
using DDD.Infrastructure.Ports.Adapters.Ado;
using DDD.Application.Settings;

namespace DDD.Infrastructure.Ports.Adapters.Postgres
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
