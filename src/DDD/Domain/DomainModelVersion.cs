using System;
using DDD.Application.Exceptions;

namespace DDD.Domain
{
	public class DomainModelVersion : IDomainModelVersion, IEquatable<DomainModelVersion>
	{
		private Version _version { get; set; }

		private DomainModelVersion() {}
		
		// Public API

		public static DomainModelVersion Latest()
		{
			var version = new DomainModelVersion
			{
				_version = new Version(1, 0, 1)
			};
			return version;			
		}
		
		public static DomainModelVersion Create(string dotString)
		{
			if (dotString == null || dotString == "")
				// dotString = "9.9.9";
				throw new DddException($"The dotString must be of format 'x.x.x'. Was: '{dotString}'.");
			
			var version = new DomainModelVersion
			{
				_version = new Version(dotString)
			};
			return version;
		}

		public static DomainModelVersion Create(
			int major,
			int minor,
			int build)
		{
			var version = new DomainModelVersion
			{
				_version = new Version(major, minor, build)
			};
			return version;
		}

		public override string ToString()
			=> _version.ToString();

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
			return _version.CompareTo(other._version);
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
