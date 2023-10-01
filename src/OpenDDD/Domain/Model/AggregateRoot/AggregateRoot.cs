using System;

namespace OpenDDD.Domain.Model.AggregateRoot
{
	public abstract class AggregateRoot : Entity.Entity, IEquatable<AggregateRoot>
	{
		public BaseDomainModelVersion DomainModelVersion { get; set; }

		// Equality

		public bool Equals(AggregateRoot other)
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
			return Equals((AggregateRoot)obj);
		}

		public override int GetHashCode()
		{
			return (DomainModelVersion != null ? DomainModelVersion.GetHashCode() : 0);
		}

		public static bool operator ==(AggregateRoot left, AggregateRoot right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(AggregateRoot left, AggregateRoot right)
		{
			return !Equals(left, right);
		}
	}
}
