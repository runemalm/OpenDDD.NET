using DDD.Infrastructure.Ports.Adapters.Http.Translation;
using Application.Actions.Commands;
using Infrastructure.Ports.Adapters.Http.v1_0_0.Model.Commands;

namespace Infrastructure.Ports.Adapters.Http.v1_0_0.Translation.Commands
{
    public class PredictWeatherCommandTranslator : CommandTranslator
    {
        public PredictWeatherCommandTranslator()
        {
            
        }

        public PredictWeatherCommand From_v1_0_0(PredictWeatherCommand_v1_0_0 command_v1_0_0)
        {
            var command = new PredictWeatherCommand()
            {
                
            };

            return command;
        }
    }
}
