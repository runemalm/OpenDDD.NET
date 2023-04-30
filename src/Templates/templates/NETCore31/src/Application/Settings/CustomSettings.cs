using Application.Settings.Frontend;

namespace Application.Settings
{
	public class CustomSettings : ICustomSettings
	{
		public IFrontendSettings Frontend { get; }
		
		public CustomSettings(IFrontendSettings frontendSettings)
		{
			Frontend = frontendSettings;
		}
	}
}
