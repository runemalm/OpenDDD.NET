using System;

namespace DDD.Infrastructure.Ports.Adapters.Http
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DocsDefinitionAttribute : Attribute
    {
        public DocsDefinitionAttribute()
        {
            
        }
    }
}
