using System.Collections.Generic;
using DDD.Domain.Model.Validation;

namespace DDD.Application
{
	public interface ICommand
	{
		void Validate();
		IEnumerable<ValidationError> GetErrors();
	}
}
