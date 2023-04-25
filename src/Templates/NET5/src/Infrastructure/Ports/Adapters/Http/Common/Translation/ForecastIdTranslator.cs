using DDD.Infrastructure.Ports.Adapters.Common.Translation;
using Domain.Model.Forecast;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation
{
	public class ForecastIdTranslator : Translator
	{
		public ForecastIdTranslator()
		{
		            
		}

		public ForecastId FromV1(string forecastIdV1)
		{
			return ForecastId.Create(forecastIdV1);
		}

		public string ToV1(ForecastId forecastId)
		{
			return forecastId.Value;
		}
	}
}
