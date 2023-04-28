using OpenDDD.Infrastructure.Ports.Adapters.Http.Common;
using Application.Actions.Commands;
using Infrastructure.Ports.Adapters.Http.v1.Model.Commands;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation.Commands
{
    public class GetAverageTemperatureCommandTranslator : CommandTranslator
    {
        public GetAverageTemperatureCommand FromV1(GetAverageTemperatureCommandV1 commandV1)
        {
            var command = new GetAverageTemperatureCommand()
            {
                
            };

            return command;
        }
    }
}
