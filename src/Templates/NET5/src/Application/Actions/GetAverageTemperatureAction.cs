using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using Application.Actions.Commands;

namespace Application.Actions
{
    public class GetAverageTemperatureAction : Action<GetAverageTemperatureCommand, int>
    {
        public GetAverageTemperatureAction(ActionDependencies deps) : base(deps)
        {
            
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
