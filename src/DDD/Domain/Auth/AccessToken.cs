using System;
using DDD.Application.Settings;
using Domain.Model.AuthFlow;
using Microsoft.Extensions.Logging;

namespace DDD.Domain.Auth
{
	public abstract class AccessToken : ValueObject, IAccessToken, IEquatable<AccessToken>
	{
		public AuthMethod AuthMethod { get; set; }
		public TokenType TokenType { get; set; }
		public string RawString { get; set; }
		public DateTime ValidFrom { get; set; }
		public DateTime ValidTo { get; set; }
		public ILogger _logger { get; set; }

		public AccessToken() {}
		public AccessToken(DomainModelVersion domainModelVersion) : base(domainModelVersion) {}

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
