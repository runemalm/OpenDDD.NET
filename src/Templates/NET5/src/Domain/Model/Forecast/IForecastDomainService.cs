using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;

namespace Domain.Model.Forecast
{
	public interface IForecastDomainService
	{
		Task<int> GetAverageTemperatureAsync(ActionId actionId, CancellationToken ct);
	}
}
