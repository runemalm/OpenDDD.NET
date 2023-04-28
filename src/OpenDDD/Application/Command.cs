using System.Collections.Generic;
using OpenDDD.Domain.Model.Validation;

namespace OpenDDD.Application
{
	public abstract class Command : ICommand
	{
		public abstract void Validate();
		public abstract IEnumerable<ValidationError> GetErrors();
	}
}
