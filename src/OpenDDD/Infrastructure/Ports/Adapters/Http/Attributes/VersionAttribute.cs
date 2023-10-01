using System;

namespace OpenDDD.Infrastructure.Ports.Adapters.Http.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VersionAttribute : Attribute
    {
        public string Version;

        public VersionAttribute(string version)
        {
            Version = version;
        }
    }
}
