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
        
        public const int Authorization_Forbidden_Code = 601;
        public const string Authorization_Forbidden_Msg = "The user is lacking permissions.";
        public const string Authorization_Forbidden_UsrMsg = "It seems you tried to do something you are not authorized to do. Ask your manager for the permissions needed to execute the task and try again.";
        
        public const int Authorization_InvalidCredentials_Code = 602;
        public const string Authorization_InvalidCredentials_Msg = "The credentials was invalid.";
        public const string Authorization_InvalidCredentials_UsrMsg = "The following was wrong with the credentials you provided: {0}";
        
        public const int Authorization_MissingCredentials_Code = 603;
        public const string Authorization_MissingCredentials_Msg = "Missing credentials.";
        public const string Authorization_MissingCredentials_UsrMsg = "Couldn't perform your request because you seem not to be logged in? Couldn't find any credentials: '{0}'. Please logout and login again or try again later.";
        
        public const int Authorization_EmailNotVerified_Code = 604;
        public const string Authorization_EmailNotVerified_Msg = "Email not verified.";
        public const string Authorization_EmailNotVerified_UsrMsg = "You can't login before you have verified your email. Check your inbox for a verification email and click the link in it.";

        public const int Authorization_NotAuthenticated_Code = 605;
        public const string Authorization_NotAuthenticated_Msg = "Not authenticated.";
        public const string Authorization_NotAuthenticated_UsrMsg = "You must be logged in to perform the action.";
        
        public const int Authorization_InvalidRequest_Code = 606;
        public const string Authorization_InvalidRequest_Msg = "Invalid authorization request.";
        public const string Authorization_InvalidRequest_UsrMsg = "You tried to perform an invalid authorization request. The following was wrong with your request: {0}";
        
        public const int Authorization_TokenExpired_Code = 607;
        public const string Authorization_TokenExpired_Msg = "The access token has expired.";
        public const string Authorization_TokenExpired_UsrMsg = "The access token has expired. This happens after a longer period without activity. Please re-login and try again.";
        
        public static IDomainError Authorization_Forbidden() => Create(Authorization_Forbidden_Code, Authorization_Forbidden_Msg, Authorization_Forbidden_UsrMsg);
        public static IDomainError Authorization_InvalidCredentials(string reason) => Create(Authorization_InvalidCredentials_Code, Authorization_InvalidCredentials_Msg, String.Format(Authorization_InvalidCredentials_UsrMsg, reason));
        public static IDomainError Authorization_MissingCredentials(string spec) => Create(Authorization_MissingCredentials_Code, Authorization_MissingCredentials_Msg, String.Format(Authorization_MissingCredentials_UsrMsg, spec));
        public static IDomainError Authorization_EmailNotVerified() => Create(Authorization_EmailNotVerified_Code, Authorization_EmailNotVerified_Msg, Authorization_EmailNotVerified_UsrMsg);
        public static IDomainError Authorization_NotAuthenticated() => Create(Authorization_NotAuthenticated_Code, Authorization_NotAuthenticated_Msg, Authorization_NotAuthenticated_UsrMsg);
        public static IDomainError Authorization_InvalidRequest(string spec) => Create(Authorization_InvalidRequest_Code, Authorization_InvalidRequest_Msg, String.Format(Authorization_InvalidRequest_UsrMsg, spec));
        public static IDomainError Authorization_TokenExpired() => Create(Authorization_TokenExpired_Code, Authorization_TokenExpired_Msg, String.Format(Authorization_TokenExpired_UsrMsg));

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
