using System;

namespace DDD.Domain
{
	public class BuildingBlock : IBuildingBlock, IEquatable<BuildingBlock>
	{
		public DomainModelVersion DomainModelVersion { get; set; }

		public BuildingBlock()
		{
			DomainModelVersion = Domain.DomainModelVersion.Latest();
		}
		public BuildingBlock(DomainModelVersion domainModelVersion)
		{
			DomainModelVersion = domainModelVersion;
		}
		
		// Equality

		public bool Equals(BuildingBlock other)
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
			return Equals((BuildingBlock)obj);
		}

		public override int GetHashCode()
		{
			return (DomainModelVersion != null ? DomainModelVersion.GetHashCode() : 0);
		}

		public static bool operator ==(BuildingBlock left, BuildingBlock right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(BuildingBlock left, BuildingBlock right)
		{
			return !Equals(left, right);
		}
	}
}
