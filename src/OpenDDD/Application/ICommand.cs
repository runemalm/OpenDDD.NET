using System.Collections.Generic;
using OpenDDD.Domain.Model.Validation;

namespace OpenDDD.Application
{
	public interface ICommand
	{
		void Validate();
		IEnumerable<ValidationError> GetErrors();
	}
}
