using System;
using System.Linq;
using DDD.Domain.Model.BuildingBlocks.ValueObject;
using DDD.Domain.Model.Error;
using DDD.Domain.Model.Validation;

namespace Domain.Model.Notification
{
	public class Email : ValueObject, IEquatable<Email>
	{
		public string Value { get; set; }

        public Email() {}

		// Public

		public static Email Create(
			string value)
		{
			var email =
				new Email
				{
					Value = value
				};

			email.Validate();

			return email;
		}

		// Private

		protected void Validate()
		{
			var validator = new Validator<Email>(this);

			var errors = validator
				.Email(bb => bb.Value)
				.Errors()
				.ToList();

			if (errors.Any())
			{
				throw DomainException.InvariantViolation(
					$"Email is invalid with errors: " +
					$"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
			}
		}
		
		public override string ToString()
		{
			return Value;
		}

		// Equality

		public bool Equals(Email other)
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
			return Equals((Email)obj);
		}

		public override int GetHashCode()
		{
			return (Value != null ? Value.GetHashCode() : 0);
		}

		public static bool operator ==(Email left, Email right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Email left, Email right)
		{
			return !Equals(left, right);
		}
	}
}
