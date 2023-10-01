using System;
using System.Collections.Generic;
using System.Linq;
using OpenDDD.Domain.Model.Validation;

namespace OpenDDD.Application.Error
{
	public class InvalidCommandException : Exception
	{
		public readonly CommandBase CommandBase;
		public readonly IEnumerable<ValidationError> Errors;

		public InvalidCommandException(
			CommandBase commandBase, IEnumerable<ValidationError> errors)
			: this(commandBase, errors, null)
		{
		}

		public InvalidCommandException(
			CommandBase commandBase, IEnumerable<ValidationError> errors, Exception inner)
			: base($"The {commandBase.GetType().Name} command contained errors: " +
				   $"{string.Join(", ", errors.Select(e => e.ToString()))}", inner)
		{
			CommandBase = commandBase;
			Errors = errors;
		}
	}
}
