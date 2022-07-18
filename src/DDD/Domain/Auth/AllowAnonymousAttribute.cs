using System;

namespace DDD.Domain.Auth
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowAnonymousAttribute : Attribute
    {
        public AllowAnonymousAttribute()
        {
            
        }
    }
}
