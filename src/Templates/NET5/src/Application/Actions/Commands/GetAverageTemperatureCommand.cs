using System.Collections.Generic;
using System.Linq;
using DDD.Application;
using DDD.Application.Error;
using DDD.Domain.Model.Validation;

namespace Application.Actions.Commands
{
    public class GetAverageTemperatureCommand : Command
    {
        

        public override void Validate()
        {
            var errors = GetErrors();

            if (errors.Any())
                throw new InvalidCommandException(this, errors);
        }

        public override IEnumerable<ValidationError> GetErrors()
        {
            var errors = new Validator<GetAverageTemperatureCommand>(this)
                .Errors();

            return errors;
        }
    }
}
