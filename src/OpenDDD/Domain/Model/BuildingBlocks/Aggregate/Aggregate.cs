using System;

namespace OpenDDD.Domain.Model.BuildingBlocks.Aggregate
{
	public class Aggregate : Entity.Entity, IEquatable<Aggregate>
	{
		public DomainModelVersion DomainModelVersion { get; set; }

		public Aggregate()
		{
			
		}
		
		public Aggregate(DomainModelVersion domainModelVersion)
		{
			DomainModelVersion = domainModelVersion;
		}
		
		// Equality

		public bool Equals(Aggregate other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(DomainModelVersion, other.DomainModelVersion);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Aggregate)obj);
		}

		public override int GetHashCode()
		{
			return (DomainModelVersion != null ? DomainModelVersion.GetHashCode() : 0);
		}

		public static bool operator ==(Aggregate left, Aggregate right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Aggregate left, Aggregate right)
		{
			return !Equals(left, right);
		}
	}
}
