using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenDDD.Domain.Model.Error;
using OpenDDD.Infrastructure.Ports.Adapters.Translation.Translators;

namespace OpenDDD.Domain.Model
{
	[JsonConverter(typeof(DomainModelVersionTranslator))]
	public class BaseDomainModelVersion : IEquatable<BaseDomainModelVersion>, IComparable<BaseDomainModelVersion>
	{
		private Version _version { get; set; }

		public int Major => _version.Major;
		public int Minor => _version.Minor;
		public int Patch => _version.Build;

		public BaseDomainModelVersion(string dotString)
		{
			if (string.IsNullOrEmpty(dotString))
				throw DomainException.ModelError($"The domain model version string is not a valid dotstring: '{dotString}'.");

			_version = new Version(dotString);
		}
		
		public BaseDomainModelVersion(int major, int minor, int build)
		{
			_version = new Version(major, minor, build);
		}
		
		// Public API

		public override string ToString()
			=> _version.ToString();
		
		public string ToStringWithWildcardMinorAndPatchVersions()
			=> $"{_version.Major}.*.*";

		// Equality

		public bool Equals(BaseDomainModelVersion other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(_version, other._version);
		}
		
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType() && !obj.GetType().IsSubclassOf(GetType())) return false;
			return Equals((BaseDomainModelVersion)obj);
		}

		public override int GetHashCode()
		{
			return (_version != null ? _version.GetHashCode() : 0);
		}
		
		public static bool operator ==(BaseDomainModelVersion left, BaseDomainModelVersion right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(BaseDomainModelVersion left, BaseDomainModelVersion right)
		{
			return !Equals(left, right);
		}

		public int CompareTo(BaseDomainModelVersion other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			return Comparer<Version>.Default.Compare(_version, other._version);
		}
		
		public static bool operator <(BaseDomainModelVersion left, BaseDomainModelVersion right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(BaseDomainModelVersion left, BaseDomainModelVersion right)
		{
			return left.CompareTo(right) > 0;
		}
	}
}
