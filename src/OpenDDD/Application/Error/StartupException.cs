using System;

namespace OpenDDD.Application.Error
{
    public class StartupException : Application.Error.ApplicationException
    {
        public static StartupException Failed(string reason)
            => new StartupException(ApplicationError.Startup_Failed(reason));
        
        public StartupException(IApplicationError error) : base(error)
        {
            
        }
        
        public StartupException(IApplicationError error, Exception inner) : base(error, inner)
        {
            
        }
    }
}
