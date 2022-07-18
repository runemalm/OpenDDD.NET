using DDD.Infrastructure.Ports.Adapters.Http.Translation;
using Application.Actions.Commands;
using Infrastructure.Ports.Adapters.Http.{{api_version}}.Model.Commands;

namespace Infrastructure.Ports.Adapters.Http.{{api_version}}.Translation.Commands
{
    public class {{action_name}}CommandTranslator : CommandTranslator
    {
        public {{action_name}}CommandTranslator()
        {
            
        }

        public {{action_name}}Command From_{{api_version}}({{action_name}}Command_{{api_version}} command_{{api_version}})
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
