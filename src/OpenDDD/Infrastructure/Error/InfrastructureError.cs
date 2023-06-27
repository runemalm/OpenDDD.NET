using System;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace OpenDDD.Infrastructure.Error
{
    public class InfrastructureError : IInfrastructureError, IEquatable<InfrastructureError>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string UserMessage { get; set; }
        
        // Constructor
        
        public static IInfrastructureError Create(int code, string message, string userMessage)
            => new InfrastructureError
            {
                Code = code,
                Message = message,
                UserMessage = userMessage
            };
        
        // System errors

        public const int Infrastructure_DependencyFailed_Code = 801;
        public const string Infrastructure_DependencyFailed_Msg = "A dependency request failed. Reason was: '{0}'";
        public const string Infrastructure_DependencyFailed_UsrMsg = "The system experienced an internal error due to a dependency request that failed. Please try again later.";
        
        public static IInfrastructureError Infrastructure_DependencyFailed(string reason) => Create(Infrastructure_DependencyFailed_Code, String.Format(Infrastructure_DependencyFailed_Msg, reason), Infrastructure_DependencyFailed_UsrMsg);

        // Equality

        public bool Equals(InfrastructureError? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Code == other.Code && Message == other.Message && UserMessage == other.UserMessage;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InfrastructureError)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Message, UserMessage);
        }
    }
}
