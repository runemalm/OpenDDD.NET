using DDD.Application;

namespace Domain.Model.Forecast
{
	public interface IForecastDomainService
	{
		Task<int> GetAverageTemperatureAsync(ActionId actionId, CancellationToken ct);
	}
}
