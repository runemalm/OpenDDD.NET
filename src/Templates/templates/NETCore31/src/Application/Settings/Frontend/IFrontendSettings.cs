namespace Application.Settings.Frontend
{
	public interface IFrontendSettings
	{
		string BaseUrl { get; }
		string PathResetPassword { get; }
		string PathVerifyEmail { get; }
	}
}
