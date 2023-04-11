using System.Collections.Generic;
using DDD.Domain.Model.Error;

namespace DDD.Application.Error
{
	public class Failure
	{
		public IEnumerable<IDomainError> Errors { get; }

		public Failure(IEnumerable<IDomainError> errors)
		{
			Errors = errors;
		}

		public Failure(IDomainError error)
		{
			Errors = new List<IDomainError> { error };
		}
	}
}
