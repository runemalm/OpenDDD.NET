using OpenDDD.Application.Error;

namespace OpenDDD.NET.Exceptions
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
