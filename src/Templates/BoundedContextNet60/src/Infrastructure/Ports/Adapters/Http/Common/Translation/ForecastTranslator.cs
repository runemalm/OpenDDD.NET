using DDD.Infrastructure.Ports.Adapters.Common.Translation;
using Domain.Model.Forecast;
using Domain.Model.Summary;
using Infrastructure.Ports.Adapters.Http.v1.Model;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation;

public class ForecastTranslator : Translator
{
	public Forecast FromV1(ForecastV1 forecastV1)
	{
		return new Forecast
		{
			ForecastId = ForecastId.Create(forecastV1.ForecastId),
			Date = forecastV1.Date,
			TemperatureC = forecastV1.TemperatureC,
			SummaryId = SummaryId.Create(forecastV1.SummaryId)
		};
	}

	public ForecastV1 ToV1(Forecast forecast)
	{
		return new ForecastV1
		{
			ForecastId = forecast.ForecastId.Value,
			Date = forecast.Date,
			TemperatureC = forecast.TemperatureC,
			TemperatureF = forecast.TemperatureF,
			SummaryId = forecast.SummaryId.Value
		};
	}

	public IEnumerable<ForecastV1> ToV1(IEnumerable<Forecast> forecasts)
		=> forecasts.Select(ToV1);
}
