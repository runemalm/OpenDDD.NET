using System;

namespace DDD.Domain.Model.Auth
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowAnonymousAttribute : Attribute
    {
        public AllowAnonymousAttribute()
        {
            
        }
    }
}
