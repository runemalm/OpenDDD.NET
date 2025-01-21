using Microsoft.Extensions.DependencyInjection;

namespace OpenDDD.Main.Attributes
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
