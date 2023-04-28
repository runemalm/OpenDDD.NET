using OpenDDD.Infrastructure.Ports.Adapters.Repository;
using Domain.Model.Forecast;
using WeatherDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Infrastructure.Ports.Adapters.Repositories.Migration
{
	public class ForecastMigrator : Migrator<Forecast>
	{
		public ForecastMigrator() : base(WeatherDomainModelVersion.Latest())
		{
			
		}
	}
}
