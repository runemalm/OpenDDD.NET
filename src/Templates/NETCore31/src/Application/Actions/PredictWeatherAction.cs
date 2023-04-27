using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain.Services.Auth;
using DDD.Infrastructure.Ports.PubSub;
using Application.Actions.Commands;
using Domain.Model.Forecast;
using Domain.Model.Summary;

namespace Application.Actions
{
    public class PredictWeatherAction : Action<PredictWeatherCommand, Forecast>
    {
        private readonly IAuthDomainService _authDomainService;
        private readonly IDomainPublisher _domainPublisher;
        private readonly IForecastRepository _forecastRepository;
        private readonly IIcForecastTranslator _icForecastTranslator;
        private readonly IInterchangePublisher _interchangePublisher;
        private readonly ISummaryRepository _summaryRepository;

        public PredictWeatherAction(
            IAuthDomainService authDomainService,
            IDomainPublisher domainPublisher,
            IForecastRepository forecastRepository,
            IIcForecastTranslator icForecastTranslator,
            IInterchangePublisher interchangePublisher,
            ISummaryRepository summaryRepository,
            ITransactionalDependencies transactionalDependencies)
            : base(transactionalDependencies)
        {
            _authDomainService = authDomainService;
            _domainPublisher = domainPublisher;
            _forecastRepository = forecastRepository;
            _icForecastTranslator = icForecastTranslator;
            _interchangePublisher = interchangePublisher;
            _summaryRepository = summaryRepository;
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
