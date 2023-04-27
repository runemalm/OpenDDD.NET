using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain.Services;
using DDD.Domain.Services.Auth;

namespace Domain.Model.Forecast
{
	public class ForecastDomainService : DomainService, IForecastDomainService
	{
		private readonly IAuthDomainService _authDomainService;
		private readonly IForecastRepository _forecastRepository;
		
		public ForecastDomainService(
			IAuthDomainService authDomainService,
			IForecastRepository forecastRepository)
		{
			_authDomainService = authDomainService;
			_forecastRepository = forecastRepository;
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
			
			var averageTempC = Convert.ToInt32(forecasts.Average(f => f.TemperatureC));

			return averageTempC;
		}
	}
}
