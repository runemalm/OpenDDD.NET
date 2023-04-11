using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DDD.Domain.Model.Auth.Exceptions;
using DDD.Domain.Model.Error;
using DDD.Domain.Model.Validation;
using DDD.NETCore.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace DDD.Domain.Model.Auth
{
	public class JwtToken : AccessToken, IEquatable<JwtToken>
	{
		public string UserId { get; set; }
		public IEnumerable<string> Audiences { get; set; }
		public string Issuer { get; set; }
		public DateTime ValidFrom { get; set; }
		public DateTime ValidTo { get; set; }
		public IEnumerable<string> Roles { get; set; }

		// Public
		
		public static JwtToken Create(
			string userId,
			AuthMethod authMethod,
			IEnumerable<string> audiences,
			string issuer,
			DateTime validFrom,
			DateTime validTo,
			IEnumerable<string> roles,
			IEnumerable<string> rolesClaimsTypes,
			string privateKey)
		{
			var token = new JwtToken()
			{
				UserId = userId,
				AuthMethod = authMethod,
				TokenType = TokenType.JWT,
				Audiences = audiences,
				Issuer = issuer,
				ValidFrom = validFrom,
				ValidTo = validTo,
				Roles = roles
			};
			token.Write(privateKey, rolesClaimsTypes);
			token.Validate(privateKey);
  
			return token;
		}
		
		public static JwtToken Read(
			string jwtString, 
			IEnumerable<string>? rolesClaimsTypes = null, 
			AuthMethod authMethod = AuthMethod.Unknown)
		{
			var handler = new JwtSecurityTokenHandler();
			JwtSecurityToken secToken = handler.ReadJwtToken(jwtString);

			var jwtToken =
				new JwtToken()
				{
					UserId = null,
					AuthMethod = authMethod,
					TokenType = TokenType.JWT,
					RawString = jwtString,
					Audiences = secToken.Audiences,
					Issuer = secToken.Issuer,
					ValidFrom = secToken.ValidFrom,
					ValidTo = secToken.ValidTo,
					Roles = null
				};

			if (authMethod == AuthMethod.Unknown)
				jwtToken.ReadAuthMethod(secToken.Claims);
			jwtToken.ReadRoles(secToken.Claims, rolesClaimsTypes ?? new List<string>());
			jwtToken.ReadUserId(secToken.Claims);

			return jwtToken;
		}

		public void Write(string privateKey, IEnumerable<string> rolesClaimsTypes)
		{
			var claims = ClaimsFromRoles(Roles, rolesClaimsTypes).ToList();
			
			claims.AddRange(
				new List<Claim>(){ ClaimFromAuthMethod(AuthMethod) });
			
			claims.AddRange(
				new List<Claim>(){ ClaimFromUserId(UserId) });
			
			var jwtPayload = 
				new JwtPayload(
					Issuer, 
					Audiences.FirstOrDefault(), 
					claims,
					ValidFrom, 
					ValidTo);

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
		
		private void ReadAuthMethod(IEnumerable<Claim> claims)
		{
			var found = false;
			
			foreach (var claim in claims)
				if (claim.Type == "AuthMethod")
				{
					found = true;
					if (!Enum.TryParse<AuthMethod>(claim.Value, out var authMethod))
						throw AuthorizeException.InvalidCredentials("Couldn't parse auth method from claim.");
					AuthMethod = authMethod;
					break;
				}
			if (!found)
				AuthMethod = AuthMethod.Unknown;
		}

		private void ReadRoles(IEnumerable<Claim> claims, IEnumerable<string> rolesClaimsTypes)
		{
			var roles = new List<string>();

			foreach (var claim in claims)
				if (rolesClaimsTypes.Contains(claim.Type))
					roles.Add(claim.Value.ToLower());

			Roles = roles;
		}
		
		private void ReadUserId(IEnumerable<Claim> claims)
		{
			foreach (var claim in claims)
				if (claim.Type == "UserId")
					UserId = claim.Value;
		}

		private IEnumerable<Claim> ClaimsFromRoles(IEnumerable<string> roles, IEnumerable<string> rolesClaimsTypes)
		{
			var claims = new List<Claim>();

			if (rolesClaimsTypes.Count() > 0)
			{
				foreach (var role in roles)
					claims.Add(
						new Claim(
							rolesClaimsTypes.First(), 
							role));
			}

			return claims;
		}
		
		private Claim ClaimFromAuthMethod(AuthMethod authMethod)
			=> new Claim("AuthMethod", authMethod.ToString());

		private Claim ClaimFromUserId(string userId)
			=> new Claim("UserId", userId);

		// Validation

		public void Validate(string privateKey)
		{
			CheckSignature(privateKey);
			CheckExpired();
		}

		public void Validate(string privateKey, IEnumerable<IEnumerable<string>> roles)
		{
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
				throw DomainException.InvariantViolation(
					$"JwtToken is invalid with errors: " +
					$"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
			}
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
				throw AuthorizeException.InvalidCredentials("The token signature was invalid.");
			}
		}
		
		private void CheckExpired()
		{
			var now = DateTime.UtcNow;
		
			var expired = ValidTo < now;
			var periodStarted = ValidFrom < now;
			
			if (expired)
				throw AuthorizeException.InvalidCredentials("The token has expired.");
		
			if (!periodStarted)
				throw AuthorizeException.InvalidCredentials("The token is not valid yet.");
		}

		// Equality
		
		public bool Equals(JwtToken other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return base.Equals(other) && Equals(Audiences, other.Audiences) && Issuer == other.Issuer && ValidFrom.Equals(other.ValidFrom) && ValidTo.Equals(other.ValidTo) && Equals(Roles, other.Roles);
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
			return HashCode.Combine(base.GetHashCode(), Audiences, Issuer, ValidFrom, ValidTo, Roles);
		}

		public static bool operator ==(JwtToken left, JwtToken right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(JwtToken left, JwtToken right)
		{
			return !Equals(left, right);
		}
	}
}
