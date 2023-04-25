using Domain.Model.Forecast;
using Domain.Model.Summary;
using Interchange.Domain.Model.Forecast;

namespace Infrastructure.Ports.Adapters.Interchange.Translation
{
	public class IcForecastTranslator : IIcForecastTranslator
	{
		public Forecast From(IcForecast icForecast)
		{
			return new Forecast()
			{
				ForecastId = ForecastId.Create(icForecast.ForecastId),
				Date = icForecast.Date,
				TemperatureC = icForecast.TemperatureC,
				SummaryId = SummaryId.Create(icForecast.SummaryId)
			};
		}

		public IcForecast To(Forecast forecast)
		{
			return new IcForecast()
			{
				ForecastId = forecast.ForecastId.ToString(),
				Date = forecast.Date,
				TemperatureC = forecast.TemperatureC,
				SummaryId = forecast.SummaryId.Value
			};
		}
	}
}
