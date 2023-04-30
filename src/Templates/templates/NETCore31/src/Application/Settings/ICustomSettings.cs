using Application.Settings.Frontend;

namespace Application.Settings
{
	public interface ICustomSettings
	{
		IFrontendSettings Frontend { get; }
	}
}
