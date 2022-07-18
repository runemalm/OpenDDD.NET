using System;

namespace DDD.Infrastructure.Ports.Adapters.Http
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DeprecatedAttribute : Attribute
    {
        public DeprecatedAttribute()
        {
            
        }
    }
}
