using System;
using System.Collections.Generic;
using DDD.Domain.Model.Error;


namespace DDD.Domain.Model
{
	public class DomainModelVersion : IEquatable<DomainModelVersion>, IComparable<DomainModelVersion>
	{
		private Version _version { get; set; }

		public DomainModelVersion(string dotString)
		{
			if (string.IsNullOrEmpty(dotString))
				throw DomainException.ModelError($"The domain model version string is not a valid dotstring: '{dotString}'.");

			_version = new Version(dotString);
		}
		
		public DomainModelVersion(int major, int minor, int build)
		{
			_version = new Version(major, minor, build);
		}
		
		// Public API

		public override string ToString()
			=> _version.ToString();
		
		public string ToStringWithWildcardBuild()
			=> $"{_version.Major}.{_version.Minor}.*";

		// Equality

		public bool Equals(DomainModelVersion other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(_version, other._version);
		}
		
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DomainModelVersion)obj);
		}

		public override int GetHashCode()
		{
			return (_version != null ? _version.GetHashCode() : 0);
		}
		
		public static bool operator ==(DomainModelVersion left, DomainModelVersion right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DomainModelVersion left, DomainModelVersion right)
		{
			return !Equals(left, right);
		}

		public int CompareTo(DomainModelVersion other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			return Comparer<Version>.Default.Compare(_version, other._version);
		}
		
		public static bool operator <(DomainModelVersion left, DomainModelVersion right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(DomainModelVersion left, DomainModelVersion right)
		{
			return left.CompareTo(right) > 0;
		}
	}
}
