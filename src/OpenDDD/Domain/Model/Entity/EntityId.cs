using System;
using System.Linq;
using Newtonsoft.Json;
using OpenDDD.Application.Error;
using OpenDDD.Domain.Model.Error;
using OpenDDD.Domain.Model.Validation;
using OpenDDD.Infrastructure.Ports.Adapters.Translation.Translators;

namespace OpenDDD.Domain.Model.Entity
{
	[JsonConverter(typeof(EntityIdTranslator))]
	public abstract class EntityId : ValueObject.ValueObject, IEntityId, IEquatable<EntityId>
	{
		public readonly string Value;
		
		public EntityId() { }

		public EntityId(string value)
		{
			if (value == null)
				throw new DddException("An entity ID can't be null.");
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
				throw DomainException.InvariantViolation(
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
