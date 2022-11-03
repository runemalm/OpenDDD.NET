using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDD.Domain.Model.BuildingBlocks.ValueObject;

namespace DDD.Domain.Model.Auth
{
	public class IdToken : ValueObject, IIdToken, IEquatable<IdToken>
	{
		public string Actor { get; set; }
		public IEnumerable<string> Audiences { get; set; }
		public IEnumerable<Claim> Claims { get; set; }
		public string Issuer { get; set; }
		public DateTime ValidFrom { get; set; }
		public DateTime ValidTo { get; set; }

		// Public
		
		public static IdToken Read(string jwtString)
		{
			var handler = new JwtSecurityTokenHandler();
			JwtSecurityToken secToken = handler.ReadJwtToken(jwtString);

			var idToken =
				new IdToken()
				{
					Actor = secToken.Actor,
					Audiences = secToken.Audiences,
					Claims = secToken.Claims,
					Issuer = secToken.Issuer,
					ValidFrom = secToken.ValidFrom,
					ValidTo = secToken.ValidTo
				};

			return idToken;
		}

		public IEnumerable<string> GetClaimsValues(IEnumerable<string> types)
		{
			var roles = new List<string>();
			
			foreach (var type in types)
			{
				var claimValue = GetClaimListValue(type);
				if (claimValue != null)
				{
					roles.AddRange(claimValue);
				}
			}

			return roles;
		}

		public string GetClaimValue(string type)
		{
			foreach (var claim in Claims)
			{
				if (claim.Type == type)
					return claim.Value;
			}
			return null;
		}
		
		public IEnumerable<string> GetClaimListValue(string type)
		{
			var value = new List<string>();
			
			foreach (var claim in Claims)
			{
				if (claim.Type == type)
					value.Add(claim.Value);
			}
			return value;
		}
		
		// Equality

		public bool Equals(IdToken other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Actor == other.Actor && Equals(Audiences, other.Audiences) && Equals(Claims, other.Claims);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((IdToken)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Actor, Audiences, Claims);
		}

		public static bool operator ==(IdToken left, IdToken right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(IdToken left, IdToken right)
		{
			return !Equals(left, right);
		}
	}
}
