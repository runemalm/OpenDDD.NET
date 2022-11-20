using System;
using DDD.Domain.Model.BuildingBlocks.ValueObject;

namespace DDD.Domain.Model.Auth
{
	public abstract class AccessToken : ValueObject, IAccessToken, IEquatable<AccessToken>
	{
		public AuthMethod AuthMethod { get; set; }
		public TokenType TokenType { get; set; }
		public string RawString { get; set; }

		// Equality
		
		public bool Equals(AccessToken other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return AuthMethod == other.AuthMethod && TokenType == other.TokenType && RawString == other.RawString;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((AccessToken)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)AuthMethod, (int)TokenType, RawString);
		}

		public static bool operator ==(AccessToken left, AccessToken right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(AccessToken left, AccessToken right)
		{
			return !Equals(left, right);
		}
	}
}
