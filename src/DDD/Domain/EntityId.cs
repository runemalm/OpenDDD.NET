using System;
using System.Linq;
using DDD.Domain.Exceptions;
using DDD.Domain.Validation;

namespace DDD.Domain
{
	public abstract class EntityId : ValueObject, IEntityId, IEquatable<EntityId>
	{
		public readonly string Value;
		
		public EntityId(string value)
		{
			Value = value;
		}

		// Private

		protected void Validate(string entityTypeName)
		{
			var validator = new Validator<EntityId>(this);

			var errors = validator
				.NotNullOrEmpty(bb => bb.Value)
				.Errors()
				.ToList();

			if (errors.Any())
			{
				throw new InvariantException(
					$"{entityTypeName} is invalid with errors: " +
					$"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
			}
		}

		public override string ToString()
		{
			return Value;
		}

		// Equality
		
		public bool Equals(EntityId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Value == other.Value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((EntityId)obj);
		}

		public override int GetHashCode()
		{
			return (Value != null ? Value.GetHashCode() : 0);
		}

		public static bool operator ==(EntityId left, EntityId right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(EntityId left, EntityId right)
		{
			return !Equals(left, right);
		}
	}
}
