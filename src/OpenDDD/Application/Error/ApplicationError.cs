using System;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace OpenDDD.Application.Error
{
    public class ApplicationError : IApplicationError, IEquatable<ApplicationError>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string UserMessage { get; set; }
        
        // Constructor
        
        public static IApplicationError Create(int code, string message, string userMessage)
            => new ApplicationError
            {
                Code = code,
                Message = message,
                UserMessage = userMessage
            };
        
        // System errors

        public const int System_UnknownError_Code = 101;
        public const string System_UnknownError_Msg = "An unknown error has occured.";
        public const string System_UnknownError_UsrMsg = "The system experienced an unknown internal error. Sorry for the inconvenience this may have caused you. Please try again later.";
        
        public const int System_InternalError_Code = 102;
        public const string System_InternalError_Msg = "Internal system error: {0}.";
        public const string System_InternalError_UsrMsg = "The system experienced an internal error. Sorry for the inconvenience this may have caused you. Please try again later. Error details: {0}";
        
        public static IApplicationError System_UnknownError(string message) => Create(System_UnknownError_Code, String.Format(System_UnknownError_Msg, message), String.Format(System_UnknownError_UsrMsg, message));
        public static IApplicationError System_InternalError(string spec) => Create(System_InternalError_Code, String.Format(System_InternalError_Msg, spec), String.Format(System_InternalError_UsrMsg, spec));
        
        // Startup errors
        
        public const int Startup_Failed_Code = 141;
        public const string Startup_Failed_Msg = "Startup failed with error: '{0}'.";
        public const string Startup_Failed_UsrMsg = "The system has a bug. Application failed to start with error: '{0}'.";
        
        public static IApplicationError Startup_Failed(string reason) => Create(Startup_Failed_Code, String.Format(Startup_Failed_Msg, reason), String.Format(Startup_Failed_UsrMsg, reason));
        
        // Transactional errors
        
        public const int Transactional_Failed_Code = 151;
        public const string Transactional_Failed_Msg = "Transactional failed with error: '{0}'.";
        public const string Transactional_Failed_UsrMsg = "The system has a bug. Application failed to perform action transactionally with error: '{0}'.";
        
        public const int Transactional_NotRegistered_Code = 152;
        public const string Transactional_NotRegistered_Msg = "Transactional failed because the ITransactional hasn't been registered.";
        public const string Transactional_NotRegistered_UsrMsg = "The system has a bug. Application failed to perform action because it hasn't been configured with transactional correctly.";
        
        public static IApplicationError Transactional_Failed(string reason) => Create(Transactional_Failed_Code, String.Format(Transactional_Failed_Msg, reason), String.Format(Transactional_Failed_UsrMsg, reason));
        public static IApplicationError Transactional_NotRegistered() => Create(Transactional_NotRegistered_Code, String.Format(Transactional_NotRegistered_Msg), String.Format(Transactional_NotRegistered_UsrMsg));
        
        // Settings errors
		
        public const int Settings_Invalid_Code = 171;
        public const string Settings_Invalid_Msg = "Invalid setting: '{0}'";
        public const string Settings_Invalid_UsrMsg = "Couldn't perform your request because the server was misconfigured: '{0}'";

        public const int Settings_UnsupportedJwtTokenLocationSetting_Code = 172;
        public const string Settings_UnsupportedJwtTokenLocationSetting_Msg = "Unsupported jwt token location setting: {0}";
        public const string Settings_UnsupportedJwtTokenLocationSetting_UsrMsg = "The server was misconfigured. The JWT location setting is unsupported: '{0}'.";
        
        public const int Settings_AuthEnabledButNoPrivateKey_Code = 173;
        public const string Settings_AuthEnabledButNoPrivateKey_Msg = "Setting 'auth' enabled with no or empty private key is not allowed.";
        public const string Settings_AuthEnabledButNoPrivateKey_UsrMsg = "Setting 'auth' enabled with no or empty private key is not allowed.";

        public static IApplicationError Settings_Invalid(string spec) => Create(Settings_Invalid_Code, String.Format(Settings_Invalid_Msg, spec), String.Format(Settings_Invalid_UsrMsg, spec));
        public static IApplicationError Settings_UnsupportedJwtTokenLocationSetting(string location) => Create(Settings_UnsupportedJwtTokenLocationSetting_Code, String.Format(Settings_UnsupportedJwtTokenLocationSetting_Msg, location), String.Format(Settings_UnsupportedJwtTokenLocationSetting_UsrMsg, location));
        public static IApplicationError Settings_AuthEnabledButNoPrivateKey() => Create(Settings_AuthEnabledButNoPrivateKey_Code, Settings_AuthEnabledButNoPrivateKey_Msg, Settings_AuthEnabledButNoPrivateKey_UsrMsg);
        
        // Equality

        public bool Equals(ApplicationError? other)
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
            return Equals((ApplicationError)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Message, UserMessage);
        }
    }
}
