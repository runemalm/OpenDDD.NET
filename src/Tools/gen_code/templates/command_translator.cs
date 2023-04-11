using DDD.Infrastructure.Ports.Adapters.Http.Common;
using Application.Actions.Commands;
using Infrastructure.Ports.Adapters.Http.vX.Model.Commands;

namespace Infrastructure.Ports.Adapters.Http.Common.Translation.Commands
{
    public class {{action_name}}CommandTranslator : CommandTranslator
    {
        // private readonly DummyTranslator _dummyTranslator;

        public {{action_name}}CommandTranslator()
        {
            // _dummyTranslator = dummyTranslator;
        }

        public {{action_name}}Command FromVX({{action_name}}CommandVX commandVX)
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
