using OpenDddConversionSettings = OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.ConversionSettings;

namespace Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class ConversionSettings : OpenDddConversionSettings
    {
        public ConversionSettings()
        {
            Converters.Add(new EmailConverter());
        }
    }
}
