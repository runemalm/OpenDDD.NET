using System.Collections.Generic;
using System.Linq;
using DDD.Application;
using DDD.Domain.Exceptions;
using DDD.Domain.Validation;

namespace Application.Actions.Commands
{
    public class PredictWeatherCommand : Command
    {
        

        public override void Validate()
        {
            var errors = GetErrors();

            if (errors.Any())
                throw new InvalidCommandException(this, errors);
        }

        public override IEnumerable<ValidationError> GetErrors()
        {
            var errors = new Validator<PredictWeatherCommand>(this)
                // .NotNullOrEmpty(command => command.xxxId.Value)
                .Errors();

            return errors;
        }
    }
}
