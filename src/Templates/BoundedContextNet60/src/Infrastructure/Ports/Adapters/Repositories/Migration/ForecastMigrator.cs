using DDD.Domain;
using Domain.Model.Forecast;
using WeatherDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Infrastructure.Ports.Adapters.Repositories.Migration
{
	public class ForecastMigrator : IMigrator<Forecast>
	{
		private readonly DomainModelVersion _latestVersion;
		
		public ForecastMigrator()
		{
			_latestVersion = WeatherDomainModelVersion.Latest();
		}

		public Forecast Migrate(Forecast forecast)
		{
			DomainModelVersion at = forecast.DomainModelVersion;

			while (at < _latestVersion)
			{
				var methodName = $"From_v{at.ToString().Replace('.', '_')}";
				var method = GetType().GetMethod(methodName, new [] {typeof(Forecast)});
				forecast = (Forecast)method.Invoke(this, new object[]{forecast});
				at = forecast.DomainModelVersion;
			}

			return forecast;
		}
		
		public IEnumerable<Forecast> Migrate(IEnumerable<Forecast> forecasts)
			=> forecasts.Select(Migrate);
	}
}
