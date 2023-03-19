using System.Collections.Generic;
using System.Linq;
using DDD.Application;
using DDD.Application.Error;

namespace DDD.Domain.Model.Validation
{
	public abstract class CommandValidator<T> where T : Command
	{
		public abstract IEnumerable<ValidationError> GetErrors(T command);

		public void Validate(T command)
		{
			var errors = GetErrors(command);

			if (errors.Any())
				throw new InvalidCommandException(command, errors);
		}
	}
}
