using DDD.Infrastructure.Persistence;
using DDD.Infrastructure.Ports.Adapters.Postgres;
using DDD.Application.Settings;
using Domain.Model.Forecast;
using Infrastructure.Ports.Adapters.Repositories.Migration;

namespace Infrastructure.Ports.Adapters.Repositories.Postgres
{
	public class PostgresForecastRepository : PostgresRepository<Forecast, ForecastId>, IForecastRepository
	{
		public PostgresForecastRepository(ISettings settings, ForecastMigrator migrator, IPersistenceService persistenceService) 
			: base(settings, "forecasts", migrator, persistenceService)
		{
			
		}
	}
}
