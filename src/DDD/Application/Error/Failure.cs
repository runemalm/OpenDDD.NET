using System.Collections.Generic;
using DDD.NETCore.Exceptions;

namespace DDD.Application.Error
{
	public class Failure
	{
		public IEnumerable<IError> Errors { get; }

		public Failure(IEnumerable<IError> errors)
		{
			Errors = errors;
		}

		public Failure(IError error)
		{
			Errors = new List<IError> { error };
		}
	}
}
