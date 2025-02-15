using Microsoft.Extensions.DependencyInjection;

namespace OpenDDD.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class LifetimeAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        public LifetimeAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}
