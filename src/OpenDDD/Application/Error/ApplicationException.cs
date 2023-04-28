using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDDD.Application.Error
{
    public class ApplicationException : DddException, IEquatable<ApplicationException>
    {
        public IEnumerable<IApplicationError> Errors;
        
        public ApplicationException(IApplicationError error)
            : base($"Application exception with error: {error.Code} ({error.Message})")
        {
            Errors = new List<IApplicationError> { error };
        }

        public ApplicationException(IEnumerable<IApplicationError> errors)
            : base($"Application exception with ({errors.Count()}) errors: {String.Join(',', errors.Select(e => $"({errors.ToList().IndexOf(e)}): {e.Code} ({e.Message})"))}")
        {
            Errors = errors;
        }
        
        public ApplicationException(IApplicationError error, Exception inner)
            : base($"Application exception with error: {error.Code} ({error.Message})", inner)
        {
            Errors = new List<IApplicationError> { error };
        }

        // Equality


        public bool Equals(ApplicationException? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Errors.Equals(other.Errors);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationException)obj);
        }

        public override int GetHashCode()
        {
            return Errors.GetHashCode();
        }
    }
}