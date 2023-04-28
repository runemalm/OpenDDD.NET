using System;
using System.Collections.Generic;
using System.Linq;
using OpenDDD.Application.Error;

namespace OpenDDD.Domain.Model.Error
{
    public class DomainException : DddException, IEquatable<DomainException>
    {
        public IEnumerable<IDomainError> Errors;
        
        public static DomainException NotFound(string entityName, string entityId)
            => new DomainException(DomainError.Domain_NotFound(entityName, entityId));
        
        public static DomainException AlreadyExists(string entityName, string propertyName, object propertyValue)
            => new DomainException(DomainError.Domain_AlreadyExists(entityName, propertyName, propertyValue));
        
        public static DomainException AlreadyExists(string entityName, string[] propertyNames, string[] propertyValues)
            => new DomainException(DomainError.Domain_AlreadyExists(entityName, string.Join(", ", propertyNames), string.Join(", ", propertyValues)));
        
        public static DomainException ModelError(string reason)
            => new DomainException(DomainError.Domain_ModelError(reason));
        
        public static DomainException InvariantViolation(string reason)
            => new DomainException(DomainError.Domain_InvariantViolation(reason));

        public DomainException(IDomainError error)
            : base($"Domain exception with error: {error.Code} ({error.Message})")
        {
            Errors = new List<IDomainError> { error };
        }

        public DomainException(IEnumerable<IDomainError> errors)
            : base($"Domain exception with ({errors.Count()}) errors: {String.Join(',', errors.Select(e => $"({errors.ToList().IndexOf(e)}): {e.Code} ({e.Message})"))}")
        {
            Errors = errors;
        }

        // Equality

        public bool Equals(DomainException? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Errors.SequenceEqual(other.Errors);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DomainException)obj);
        }

        public override int GetHashCode()
        {
            return Errors.GetHashCode();
        }
    }
}