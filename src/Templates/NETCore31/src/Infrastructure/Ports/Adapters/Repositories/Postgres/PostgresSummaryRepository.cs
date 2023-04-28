using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Adapters.Repository.Postgres;
using OpenDDD.Infrastructure.Services.Persistence;
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
		
		public Task<Summary> GetWithValueAsync(string value, ActionId actionId, CancellationToken ct)
			=> GetFirstOrDefaultWithAsync(s => s.Value == value, actionId, ct);
	}
}
