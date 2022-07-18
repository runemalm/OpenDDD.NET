using System.Linq;
using DDD.Domain.Exceptions;
using DDD.Domain.Validation;

namespace DDD.Domain.Auth
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
				throw new InvariantException(
					$"AuthorizationToken is invalid with errors: " +
					$"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
			}
		}
	}
}
