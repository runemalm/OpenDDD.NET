using System.Collections.Generic;
using DDD.Domain.Validation;

namespace DDD.Application
{
	public interface ICommand
	{
		void Validate();
		IEnumerable<ValidationError> GetErrors();
	}
}
