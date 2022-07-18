using System.Collections.Generic;

namespace DDD.Application
{
	public class Failure
	{
		public IEnumerable<Error> Errors { get; }

		public Failure(IEnumerable<Error> errors)
		{
			Errors = errors;
		}
	}
}
