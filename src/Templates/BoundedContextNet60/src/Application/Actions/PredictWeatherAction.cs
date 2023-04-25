using DDD.Application;
using Application.Actions.Commands;
using Domain.Model.Forecast;

namespace Application.Actions
{
    public class PredictWeatherAction : Action<PredictWeatherCommand, Forecast>
    {
        public PredictWeatherAction(ActionDependencies deps) : base(deps)
        {
            
        }

        public override async Task<Forecast> ExecuteAsync(
            PredictWeatherCommand command,
            ActionId actionId,
            CancellationToken ct)
        {
            // Authorize
            await _authDomainService.AssureRolesInTokenAsync(new[]
                {
                    new[] { "web.customer" },
                },
                actionId,
                ct);

            // Run
            var forecastId = ForecastId.Create(await _forecastRepository.GetNextIdentityAsync());
            
            var forecast = 
                await Forecast.PredictTomorrowAsync(
                    forecastId,
                    actionId,
                    _summaryRepository,
                    _domainPublisher,
                    _interchangePublisher, 
                    _icForecastTranslator,
                    ct);
            
            // Persist
            await _forecastRepository.SaveAsync(forecast, actionId, ct);
            
            // Return
            return forecast;
        }
    }
}
