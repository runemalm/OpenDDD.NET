using DDD.Application.Settings;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Adapters.Repository.Postgres;
using DDD.Infrastructure.Services.Persistence;
using Domain.Model.Forecast;
using Infrastructure.Ports.Adapters.Repositories.Migration;

namespace Infrastructure.Ports.Adapters.Repositories.Postgres
{
	public class PostgresForecastRepository : PostgresRepository<Forecast, ForecastId>, IForecastRepository
	{
		public PostgresForecastRepository(ISettings settings, ForecastMigrator migrator, IPersistenceService persistenceService, SerializerSettings serializerSettings) 
			: base(settings, "Forecasts", migrator, persistenceService, serializerSettings)
		{
			
		}
	}
}
