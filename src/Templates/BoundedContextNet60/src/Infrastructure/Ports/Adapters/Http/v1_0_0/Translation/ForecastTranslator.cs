using DDD.Domain;
using DDD.Infrastructure.Ports.Adapters;
using Domain.Model.Forecast;
using Infrastructure.Ports.Adapters.Http.v1_0_0.Model;

namespace Infrastructure.Ports.Adapters.Http.v1_0_0.Translation;

public class ForecastTranslator : Translator
{
	public Forecast From_v1_0_0(Forecast_v1_0_0 forecast_v1_0_0)
	{
		return new Forecast
		{
			ForecastId = ForecastId.Create(forecast_v1_0_0.ForecastId),
			Date = forecast_v1_0_0.Date,
			TemperatureC = forecast_v1_0_0.TemperatureC,
			Summary = forecast_v1_0_0.Summary
		};
	}

	public Forecast_v1_0_0 To_v1_0_0(Forecast forecast)
	{
		return new Forecast_v1_0_0
		{
			ForecastId = forecast.ForecastId.Value,
			Date = forecast.Date,
			TemperatureC = forecast.TemperatureC,
			TemperatureF = forecast.TemperatureF,
			Summary = forecast.Summary
		};
	}

	public IEnumerable<Forecast_v1_0_0> To_v1_0_0(IEnumerable<Forecast> forecasts)
		=> forecasts.Select(To_v1_0_0);
}
