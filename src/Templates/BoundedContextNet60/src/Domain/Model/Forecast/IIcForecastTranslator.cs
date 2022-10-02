using Interchange.Domain.Model.Forecast;

namespace Domain.Model.Forecast
{
	public interface IIcForecastTranslator
	{
		public Forecast From(IcForecast icForecast);
		public IcForecast To(Forecast forecast);
	}
}
