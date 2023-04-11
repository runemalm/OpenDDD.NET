using DDD.Domain.Model.Error;

namespace DDD.NETCore.Exceptions
{
    public class SystemException : DomainException
    {
        public static SystemException InternalError(string spec)
            => new SystemException(DomainError.System_InternalError(spec));

        public SystemException(IDomainError error) : base(error)
        {
            
        }
    }
}
