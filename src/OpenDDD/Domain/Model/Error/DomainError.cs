using System;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace OpenDDD.Domain.Model.Error
{
    public class DomainError : IDomainError, IEquatable<DomainError>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string UserMessage { get; set; }
        
        // Constructor
        
        public static IDomainError Create(int code, string message, string userMessage)
            => new DomainError
            {
                Code = code,
                Message = message,
                UserMessage = userMessage
            };
        
        // Domain errors
        
        public const int Domain_ModelError_Code = 501;
        public const string Domain_ModelError_Msg = "Domain model error: '{0}'.";
        public const string Domain_ModelError_UsrMsg = "The system has a bug. Please try again later and if the error persists, please contact customer support. Model error: '{0}'.";
        
        public const int Domain_NotFound_Code = 502;
        public const string Domain_NotFound_Msg = "The {0} with ID '{1}' couldn't be found.";
        public const string Domain_NotFound_UsrMsg = "Couldn't perform the action. The {0} with ID '{1}' couldn't be found. Please try again later or contact customer support if the problem persists.";
        
        public const int Domain_AlreadyExists_Code = 03;
        public const string Domain_AlreadyExists_Msg = "An {0} with {1} '{2}' already exists.";
        public const string Domain_AlreadyExists_UsrMsg = "Couldn't perform the action. An {0} with {1} '{2}' already exists. Please choose a unique value and try again.";
        
        public const int Domain_InvariantViolation_Code = 504;
        public const string Domain_InvariantViolation_Msg = "Invariant violation: {0}";
        public const string Domain_InvariantViolation_UsrMsg = "A business rule was violated: {0}";

        public static IDomainError Domain_ModelError(string reason) => Create(Domain_ModelError_Code, String.Format(Domain_ModelError_Msg, reason), String.Format(Domain_ModelError_UsrMsg, reason));
        public static IDomainError Domain_InvariantViolation(string reason) => Create(Domain_InvariantViolation_Code, String.Format(Domain_InvariantViolation_Msg, reason), String.Format(Domain_InvariantViolation_UsrMsg, reason));
        public static IDomainError Domain_NotFound(string entityName, string entityId) => Create(Domain_NotFound_Code, String.Format(Domain_NotFound_Msg, entityName, entityId), String.Format(Domain_NotFound_UsrMsg, entityName, entityId));
        public static IDomainError Domain_AlreadyExists(string entityName, string propertyName, object propertyValue) => Create(Domain_AlreadyExists_Code, String.Format(Domain_AlreadyExists_Msg, entityName, propertyName, propertyValue), String.Format(Domain_AlreadyExists_UsrMsg, entityName, propertyName, propertyValue));

        // Auth errors
        
        public const int Authorize_Forbidden_Code = 601;
        public const string Authorize_Forbidden_Msg = "The user is lacking permissions.";
        public const string Authorize_Forbidden_UsrMsg = "It seems you tried to do something you are not authorized to do. Ask your manager for the permissions needed to execute the task and try again.";
        
        public const int Authorize_InvalidCredentials_Code = 602;
        public const string Authorize_InvalidCredentials_Msg = "The credentials was invalid.";
        public const string Authorize_InvalidCredentials_UsrMsg = "The following was wrong with the credentials you provided: {0}";
        
        public const int Authorize_MissingCredentials_Code = 603;
        public const string Authorize_MissingCredentials_Msg = "Missing credentials.";
        public const string Authorize_MissingCredentials_UsrMsg = "Couldn't perform your request because you seem not to be logged in? Couldn't find any credentials: '{0}'. Please logout and login again or try again later.";
        
        public const int Authorize_NotAuthenticated_Code = 304;
        public const string Authorize_NotAuthenticated_Msg = "Not authenticated.";
        public const string Authorize_NotAuthenticated_UsrMsg = "You must be logged in to perform the action.";
        
        public const int Authorize_InvalidRequest_Code = 606;
        public const string Authorize_InvalidRequest_Msg = "Invalid authorization request.";
        public const string Authorize_InvalidRequest_UsrMsg = "You tried to perform an invalid authorization request. The following was wrong with your request: {0}";
        
        public static IDomainError Authorize_Forbidden() => Create(Authorize_Forbidden_Code, Authorize_Forbidden_Msg, Authorize_Forbidden_UsrMsg);
        public static IDomainError Authorize_InvalidCredentials(string reason) => Create(Authorize_InvalidCredentials_Code, Authorize_InvalidCredentials_Msg, String.Format(Authorize_InvalidCredentials_UsrMsg, reason));
        public static IDomainError Authorize_MissingCredentials(string spec) => Create(Authorize_MissingCredentials_Code, Authorize_MissingCredentials_Msg, String.Format(Authorize_MissingCredentials_UsrMsg, spec));
        public static IDomainError Authorize_NotAuthenticated() => Create(Authorize_NotAuthenticated_Code, Authorize_NotAuthenticated_Msg, Authorize_NotAuthenticated_UsrMsg);
        public static IDomainError Authorize_InvalidRequest(string spec) => Create(Authorize_InvalidRequest_Code, Authorize_InvalidRequest_Msg, String.Format(Authorize_InvalidRequest_UsrMsg, spec));

        // Equality

        public bool Equals(DomainError? other)
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
            return Equals((DomainError)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Message, UserMessage);
        }
    }
}
