namespace OpenDDD.Domain.Model.Error
{
    public class AuthorizationException : DomainException
    {
        public static AuthorizationException Forbidden()
            => new AuthorizationException(DomainError.Authorization_Forbidden());
        
        public static AuthorizationException InvalidCredentials(string reason)
            => new AuthorizationException(DomainError.Authorization_InvalidCredentials(reason));
        
        public static AuthorizationException MissingCredentials(string spec)
            => new AuthorizationException(DomainError.Authorization_MissingCredentials(spec));
        
        public static AuthorizationException EmailNotVerified()
            => new AuthorizationException(DomainError.Authorization_EmailNotVerified());
        
        public static AuthorizationException NotAuthenticated()
            => new AuthorizationException(DomainError.Authorization_NotAuthenticated());
        
        public static AuthorizationException InvalidRequest(string spec)
            => new AuthorizationException(DomainError.Authorization_InvalidRequest(spec));
        
        public static AuthorizationException TokenExpired()
            => new AuthorizationException(DomainError.Authorization_TokenExpired());

        public AuthorizationException(IDomainError error) : base(error)
        {
            
        }
    }
}