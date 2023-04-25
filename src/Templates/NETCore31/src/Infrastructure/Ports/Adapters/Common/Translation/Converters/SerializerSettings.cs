using DddSerializerSettings = DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.SerializerSettings;

namespace Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class SerializerSettings : DddSerializerSettings
    {
        public SerializerSettings()
        {
            Converters.Add(new EmailConverter());
        }
    }
}
