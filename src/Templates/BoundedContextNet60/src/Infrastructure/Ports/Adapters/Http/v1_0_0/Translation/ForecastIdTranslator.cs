using DDD.Infrastructure.Ports.Adapters;
using Domain.Model.Forecast;

namespace Infrastructure.Ports.Adapters.Http.v1_0_0.Translation;

public class ForecastIdTranslator : Translator
{
	public ForecastIdTranslator()
	{
		            
	}

	public ForecastId From_v1_0_0(string forecastId_v1_0_0)
	{
		return ForecastId.Create(forecastId_v1_0_0);
    }

	public string To_v1_0_0(ForecastId forecastId)
	{
		return forecastId.Value;
	}
}
