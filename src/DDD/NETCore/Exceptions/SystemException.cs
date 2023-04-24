using DDD.Application.Error;

namespace DDD.NETCore.Exceptions
{
    public class SystemException : ApplicationException
    {
        public static SystemException InternalError(string spec)
            => new SystemException(ApplicationError.System_InternalError(spec));

        public SystemException(IApplicationError error) : base(error)
        {
            
        }
    }
}
