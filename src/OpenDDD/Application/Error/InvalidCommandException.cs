using System;
using System.Collections.Generic;
using System.Linq;
using OpenDDD.Domain.Model.Validation;

namespace OpenDDD.Application.Error
{
	public class InvalidCommandException : Exception
	{
		public readonly Command Command;
		public readonly IEnumerable<ValidationError> Errors;

		public InvalidCommandException(
			Command command, IEnumerable<ValidationError> errors)
			: this(command, errors, null)
		{
		}

		public InvalidCommandException(
			Command command, IEnumerable<ValidationError> errors, Exception inner)
			: base($"The {command.GetType().Name} command contained errors: " +
				   $"{string.Join(", ", errors.Select(e => e.ToString()))}", inner)
		{
			Command = command;
			Errors = errors;
		}
	}
}
