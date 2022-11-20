using System;

namespace DDD.Infrastructure.Ports.Adapters.Http.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ProtectedAttribute : Attribute
    {
        
    }
}
