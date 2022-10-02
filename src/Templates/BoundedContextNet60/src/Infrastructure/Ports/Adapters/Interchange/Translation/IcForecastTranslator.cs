using Domain.Model.Forecast;
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
				Summary = icForecast.Summary
			};
		}

		public IcForecast To(Forecast forecast)
		{
			return new IcForecast()
			{
				ForecastId = forecast.ForecastId.ToString(),
				Date = forecast.Date,
				TemperatureC = forecast.TemperatureC,
				Summary = forecast.Summary
			};
		}
	}
}
