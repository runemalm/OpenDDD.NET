using OpenDDD.Infrastructure.Ports.Adapters.Http.Common;
using Application.Actions.Commands;
using Infrastructure.Ports.Adapters.Http.v1.Model.Commands;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation.Commands
{
    public class PredictWeatherCommandTranslator : CommandTranslator
    {
        public PredictWeatherCommandTranslator()
        {
            
        }

        public PredictWeatherCommand FromV1(PredictWeatherCommandV1 commandV1)
        {
            var command = new PredictWeatherCommand()
            {
                
            };

            return command;
        }
    }
}
