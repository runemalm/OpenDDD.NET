using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Adapters.Repository.Postgres;
using OpenDDD.Infrastructure.Services.Persistence;
using Domain.Model.Forecast;
using Infrastructure.Ports.Adapters.Repositories.Migration;

namespace Infrastructure.Ports.Adapters.Repositories.Postgres
{
	public class PostgresForecastRepository : PostgresRepository<Forecast, ForecastId>, IForecastRepository
	{
		public PostgresForecastRepository(ISettings settings, ForecastMigrator migrator, IPersistenceService persistenceService, ConversionSettings conversionSettings) 
			: base(settings, "Forecasts", migrator, persistenceService, conversionSettings)
		{
			
		}
	}
}
