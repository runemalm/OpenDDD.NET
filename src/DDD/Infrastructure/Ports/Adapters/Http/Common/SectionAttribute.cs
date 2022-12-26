using System;

namespace DDD.Infrastructure.Ports.Adapters.Http.Common
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SectionAttribute : Attribute
    {
        public string Name;

        public SectionAttribute(string name)
        {
            Name = name;
        }
    }
}
