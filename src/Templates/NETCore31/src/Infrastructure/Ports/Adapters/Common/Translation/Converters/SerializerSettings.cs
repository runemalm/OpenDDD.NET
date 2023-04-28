using OpenDddSerializerSettings = OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.SerializerSettings;

namespace Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class SerializerSettings : OpenDddSerializerSettings
    {
        public SerializerSettings()
        {
            Converters.Add(new EmailConverter());
        }
    }
}
