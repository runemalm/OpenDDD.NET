using DDD.Logging;
using DDD.Domain.Auth;
using DDD.Application.Settings;
using DDD.Domain;
using DDD.Infrastructure.Persistence;
using DDD.Infrastructure.Ports;
using Application.Actions.Commands;
using Domain.Model.Forecast;

namespace Application.Actions
{
    public class PredictWeatherAction : DDD.Application.Action<PredictWeatherCommand, Forecast>
    {
        private readonly IForecastRepository _forecastRepository;
        private readonly IIcForecastTranslator _icForecastTranslator;
        private readonly ISettings _settings;
        private readonly ILogger _logger;

        public PredictWeatherAction(
            IAuthDomainService authDomainService,
            IForecastRepository forecastRepository,
            IIcForecastTranslator icForecastTranslator,
            ISettings settings,
            ILogger logger,
            IDomainPublisher domainPublisher,
            IInterchangePublisher interchangePublisher,
            IOutbox outbox,
            IPersistenceService persistenceService) 
            : base(authDomainService, domainPublisher, interchangePublisher, outbox, persistenceService)
        {
            _forecastRepository = forecastRepository;
            _settings = settings;
            _logger = logger;
            _icForecastTranslator = icForecastTranslator;
        }

        public override async Task<Forecast> ExecuteAsync(
            PredictWeatherCommand command,
            ActionId actionId,
            CancellationToken ct)
        {
            // Authorize
            await _authDomainService.AuthorizeRolesAsync(new[]
                {
                    new[] { "web.customer" },
                }, 
                ct);

            // Run
            var forecastId = ForecastId.Create(await _forecastRepository.GetNextIdentityAsync());
            
            var forecast = 
                await Forecast.PredictTomorrow(
                    forecastId, 
                    actionId, 
                    _domainPublisher, 
                    _interchangePublisher, 
                    _icForecastTranslator);
            
            // Persist
            await _forecastRepository.SaveAsync(forecast, actionId, ct);
            
            // Return
            return forecast;
        }
    }
}
