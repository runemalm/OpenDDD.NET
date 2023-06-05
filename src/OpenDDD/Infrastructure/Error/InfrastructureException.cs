using System;
using System.Collections.Generic;
using System.Linq;
using OpenDDD.Application.Error;

namespace OpenDDD.Infrastructure.Error
{
    public class InfrastructureException : DddException, IEquatable<InfrastructureException>
    {
        public IEnumerable<IInfrastructureError> Errors;
        
        public static InfrastructureException DependencyFailed(string reason)
            => new InfrastructureException(InfrastructureError.Infrastructure_DependencyFailed(reason));

        public InfrastructureException(IInfrastructureError error)
            : base($"Infrastructure exception with error: {error.Code} ({error.Message})")
        {
            Errors = new List<IInfrastructureError> { error };
        }

        public InfrastructureException(IEnumerable<IInfrastructureError> errors)
            : base($"Infrastructure exception with ({errors.Count()}) errors: {String.Join(',', errors.Select(e => $"({errors.ToList().IndexOf(e)}): {e.Code} ({e.Message})"))}")
        {
            Errors = errors;
        }
        
        public InfrastructureException(IInfrastructureError error, Exception inner)
            : base($"Infrastructure exception with error: {error.Code} ({error.Message})", inner)
        {
            Errors = new List<IInfrastructureError> { error };
        }

        // Equality

        public bool Equals(InfrastructureException? other)
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
            return Equals((InfrastructureException)obj);
        }

        public override int GetHashCode()
        {
            return Errors.GetHashCode();
        }

        public static bool operator ==(InfrastructureException? left, InfrastructureException? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InfrastructureException? left, InfrastructureException? right)
        {
            return !Equals(left, right);
        }
    }
}