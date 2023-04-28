using System;

namespace OpenDDD.Domain.Model.Auth
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowAnonymousAttribute : Attribute
    {
        public AllowAnonymousAttribute()
        {
            
        }
    }
}
