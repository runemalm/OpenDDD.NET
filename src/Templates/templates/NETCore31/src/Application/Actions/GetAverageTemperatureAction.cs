using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using Application.Actions.Commands;
using Domain.Model.Forecast;

namespace Application.Actions
{
    public class GetAverageTemperatureAction : Action<GetAverageTemperatureCommand, int>
    {
        private readonly IForecastDomainService _forecastDomainService;
        
        public GetAverageTemperatureAction(
            IForecastDomainService forecastDomainService,
            ITransactionalDependencies transactionalDependencies)
            : base(transactionalDependencies)
        {
            _forecastDomainService = forecastDomainService;
        }

        public override async Task<int> ExecuteAsync(
            GetAverageTemperatureCommand command,
            ActionId actionId,
            CancellationToken ct)
        {
            // Run
            var averageTemp = 
                await _forecastDomainService.GetAverageTemperatureAsync(
                    actionId, 
                    ct);
            
            return averageTemp;
        }
    }
}
