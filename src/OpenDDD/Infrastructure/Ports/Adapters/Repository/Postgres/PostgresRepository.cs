using OpenDDD.Application.Settings;
using OpenDDD.Domain.Model.BuildingBlocks.Aggregate;
using OpenDDD.Domain.Model.BuildingBlocks.Entity;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Adapters.Repository.Sql;
using OpenDDD.Infrastructure.Ports.Repository;
using OpenDDD.Infrastructure.Services.Persistence;

namespace OpenDDD.Infrastructure.Ports.Adapters.Repository.Postgres
{
	public class PostgresRepository<T, TId> : SqlRepository<T, TId> where T : IAggregate where TId : EntityId
	{
		public PostgresRepository(ISettings settings, string tableName, IMigrator<T> migrator,
			IPersistenceService persistenceService, ConversionSettings conversionSettings) :
			base(settings, tableName, migrator, persistenceService, conversionSettings)
		{
			
		}
	}
}
