using System.Collections.Generic;
using DDD.Domain.Validation;

namespace DDD.Application
{
	public abstract class Command : ICommand
	{
		public abstract void Validate();
		public abstract IEnumerable<ValidationError> GetErrors();
	}
}
