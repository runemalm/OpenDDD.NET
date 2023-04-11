using System.Linq;
using DDD.Domain.Model.Error;
using DDD.Domain.Model.Validation;

namespace DDD.Domain.Model.Auth
{
	public class AuthorizationCode
	{
		public string Value { get; set; }

		// Public
		
		public static AuthorizationCode Create(string value)
		{
			var token = new AuthorizationCode()
			{
				Value = value
			};

			token.Validate();
			
			return token;
		}
		
		private void Validate()
		{
			var validator = new Validator<AuthorizationCode>(this);
  
			var errors = validator
				.NotNullOrEmpty(token => token.Value)
				.Errors()
				.ToList();
  
			if (errors.Any())
			{
				throw DomainException.InvariantViolation(
					$"AuthorizationToken is invalid with errors: " +
					$"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
			}
		}
	}
}
