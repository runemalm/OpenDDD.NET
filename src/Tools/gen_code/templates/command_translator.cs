using DDD.Infrastructure.Ports.Adapters.Http.Translation;
using Application.Actions.Commands;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation.Commands
{
    public class {{action_name}}CommandTranslator : CommandTranslator
    {
        public {{action_name}}CommandTranslator()
        {
            
        }

        public {{action_name}}Command From_vX_X_X({{action_name}}Command_vX_X_X command_vX_X_X)
        {
            throw new NotImplementedException();
            
            var command = new {{action_name}}Command()
            {
                {{param_expressions}}
            };

            return command;
        }
    }
}
