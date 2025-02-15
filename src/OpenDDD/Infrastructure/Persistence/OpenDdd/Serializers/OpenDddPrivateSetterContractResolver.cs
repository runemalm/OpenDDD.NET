using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers
{
    public class OpenDddPrivateSetterContractResolver : DefaultContractResolver
    {
        public OpenDddPrivateSetterContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (!property.Writable) // If property isn't writable, check for private setter
            {
                var propertyInfo = member as PropertyInfo;
                if (propertyInfo != null)
                {
                    var setter = propertyInfo.GetSetMethod(true); // Get private setter
                    if (setter != null)
                    {
                        property.Writable = true; // Allow writing to private setter
                    }
                }
            }

            return property;
        }
    }
}
