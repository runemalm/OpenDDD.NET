using DDD.Application;
using Domain.Services;

namespace Domain.Model.Forecast
{
	public class ForecastDomainService : DomainService, IForecastDomainService
	{
		private readonly IForecastRepository _forecastRepository;
		
		public ForecastDomainService(DomainServiceDependencies deps) : base(deps)
		{
			
		}

		public async Task<int> GetAverageTemperatureAsync(ActionId actionId, CancellationToken ct)
		{
			// Authorize
			await _authDomainService.AssureRolesInTokenAsync(new[]
				{
					new[] { "web.analyst" },
				},
				actionId,
				ct);
			
			// Run
			var forecasts = await _forecastRepository.GetAllAsync(actionId, ct);
			
			var averageTempC = Convert.ToInt32(Math.Floor((decimal)forecasts.Sum(f => f.TemperatureC)));

			return averageTempC;
		}
	}
}
