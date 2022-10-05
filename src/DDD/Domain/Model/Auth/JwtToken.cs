using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using DDD.Domain.Model.Auth.Exceptions;
using DDD.Domain.Model.Validation;
using DDD.NETCore.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace DDD.Domain.Model.Auth
{
	public class JwtToken : AccessToken, IEquatable<JwtToken>
	{
		public readonly string Issuer = "DddForDotNet-IAM";
		public string Audience { get; set; }
		public IEnumerable<Claim> Claims { get; set; }
		private const string Format = "[a-zA-Z0-9+/=]+\\.[a-zA-Z0-9+/=]+\\..+";

		// Public
		
		public static JwtToken Create(
			DomainModelVersion domainModelVersion, 
			AuthMethod authMethod, 
			IEnumerable<Claim> claims, 
			DateTime validFrom, 
			DateTime validTo, 
			string audience, 
			string privateKey)
		{
			var token = new JwtToken()
			{
				TokenType = TokenType.JWT,
				AuthMethod = authMethod,
				Claims = claims,
				ValidFrom = validFrom,
				ValidTo = validTo,
				Audience = audience
			};
			token.ReadRoles();
			token.Write(privateKey);
			token.Validate(privateKey);
  
			return token;
		}

		private void ReadRoles()
		{
			var roles = new List<string>();
			
			foreach (var claim in Claims)
				if (claim.Type.ToLower() == "roles")
					roles.Add(claim.Value.ToLower());

			Roles = roles;
		}
		
		public void Write(string privateKey)
		{
			var jwtPayload = new JwtPayload(Issuer, Audience, Claims, ValidFrom, ValidTo);

			var credentials = 
				new SigningCredentials(
					new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(privateKey)), 
					SecurityAlgorithms.HmacSha256);
  
			RawString =
				new JwtSecurityTokenHandler().WriteToken(
					new JwtSecurityToken(
						new JwtHeader(credentials),
						jwtPayload));
		}

		public static JwtToken Read(string rawString)
		{
			if (!ValidateFormat(rawString))
			{
				throw new AuthException(
					"Couldn't read JWT token from string. The format was invalid.");
			}

			JwtSecurityToken secToken = null;

			var handler = new JwtSecurityTokenHandler();
			try
			{
				secToken = handler.ReadJwtToken(rawString);
			}
			catch (Exception e)
			{
				
			}

			JwtToken token = new JwtToken();
					
			if (secToken != null)
			{
				token.TokenType = TokenType.JWT;
				token.AuthMethod = AuthMethod.Unknown;
				token.Claims = secToken.Claims;
				token.ValidFrom = secToken.ValidFrom;
				token.ValidTo = secToken.ValidTo;
				token.Audience = secToken.Audiences.First();
				token.RawString = rawString;
				
				token.ReadRoles();
			}

			return token;
		}

		// Validation

		public void Validate(string privateKey)
		{
			CheckFormat();
			CheckSignature(privateKey);
			CheckExpired();
		}

		public void Validate(string privateKey, IEnumerable<IEnumerable<string>> roles)
		{
			CheckFormat();
			CheckSignature(privateKey);
			CheckExpired();
			
			var validator = new Validator<AccessToken>(this);
  
			var errors = validator
				.NotNullOrEmpty(token => token.RawString)
				.Errors()
				.ToList();

			var hasRoles = roles.Any(rolesCombination => rolesCombination.IsSubsetOf(Roles));
			if (!hasRoles)
			{
				var error = new ValidationError();
				error.Key = "missing_roles";
				error.Details = "You don't have the required roles to access the resource(s).";
				errors.Add(error);
			}

			if (errors.Any())
			{
				throw new InvariantException(
					$"JwtToken is invalid with errors: " +
					$"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
			}
		}

		private static bool ValidateFormat(string rawString)
		{
			if (rawString == null)
				return false;
			return Regex.IsMatch(rawString, Format, RegexOptions.IgnoreCase);
		}

		private void CheckFormat()
		{
			if (!ValidateFormat(RawString))
				throw new InvalidCredentialsException(
					$"The token is not a valid JWT token string.");
		}

		private void CheckSignature(string privateKey)
		{
			var handler = new JwtSecurityTokenHandler();
			try
			{
				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
				var claimsPrincipal =
					handler.ValidateToken(
						RawString, new TokenValidationParameters
						{
							IssuerSigningKey = key,
							ValidateAudience = false,
							ValidateIssuer = false,
							ValidateLifetime = false // Exp validated below
						},
						out var validatedToken);
			}
			catch (SecurityTokenInvalidSignatureException e)
			{
				throw new InvalidCredentialsException(
					"The token signature was invalid.",
					e);
			}
		}
		
		private void CheckExpired()
		{
			var now = DateTime.UtcNow;
		
			var expired = ValidTo < now;
			var periodStarted = ValidFrom < now;
			
			if (expired)
				throw new InvalidCredentialsException("The token has expired.");
		
			if (!periodStarted)
				throw new InvalidCredentialsException("The token is not valid yet.");
		}

		// Equality

		public bool Equals(JwtToken other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Issuer == other.Issuer && Audience == other.Audience && Equals(Claims, other.Claims);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((JwtToken)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(base.GetHashCode(), Issuer, Audience, Claims);
		}
	}

	// internal static class RolesCombinationExtensions
	// {
	// 	internal static bool IsSubsetOf(this IEnumerable<string> rolesCombination, IEnumerable<string> roles)
	// 		=> rolesCombination?.All(role => roles.Any(r => r?.Equals(role, StringComparison.InvariantCultureIgnoreCase) ?? false)) ?? false;
	// }
}
