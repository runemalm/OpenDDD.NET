using DDD.Application;
using DDD.Application.Settings;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Adapters.Repository.Postgres;
using DDD.Infrastructure.Services.Persistence;
using Domain.Model.Summary;
using Infrastructure.Ports.Adapters.Repositories.Migration;

namespace Infrastructure.Ports.Adapters.Repositories.Postgres
{
	public class PostgresSummaryRepository : PostgresRepository<Summary, SummaryId>, ISummaryRepository
	{
		public PostgresSummaryRepository(ISettings settings, SummaryMigrator migrator, IPersistenceService persistenceService, SerializerSettings serializerSettings) 
			: base(settings, "Summaries", migrator, persistenceService, serializerSettings)
		{

		}
		
		public Task<Summary?> GetWithValueAsync(string value, ActionId actionId, CancellationToken ct)
			=> GetFirstOrDefaultWithAsync(s => s.Value == value, actionId, ct);
	}
}
