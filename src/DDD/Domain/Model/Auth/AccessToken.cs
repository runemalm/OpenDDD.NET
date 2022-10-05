using System;
using System.Collections.Generic;
using DDD.Domain.Model.BuildingBlocks;
using DDD.Domain.Model.BuildingBlocks.ValueObject;
using Microsoft.Extensions.Logging;

namespace DDD.Domain.Model.Auth
{
	public abstract class AccessToken : ValueObject, IAccessToken, IEquatable<AccessToken>
	{
		public AuthMethod AuthMethod { get; set; }
		public TokenType TokenType { get; set; }
		public string RawString { get; set; }
		public DateTime ValidFrom { get; set; }
		public DateTime ValidTo { get; set; }
		public IEnumerable<string> Roles { get; set; }
		public ILogger _logger { get; set; }

		// Equality

		public bool Equals(AccessToken other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return AuthMethod == other.AuthMethod && TokenType == other.TokenType && RawString == other.RawString && ValidFrom.Equals(other.ValidFrom) && ValidTo.Equals(other.ValidTo);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)AuthMethod, (int)TokenType, RawString, ValidFrom, ValidTo);
		}
	}
}
