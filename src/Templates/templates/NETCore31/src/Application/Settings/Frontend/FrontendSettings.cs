using System;
using Microsoft.Extensions.Options;

namespace Application.Settings.Frontend
{
	public class FrontendSettings : IFrontendSettings
	{
		public string BaseUrl { get; }
		public string PathResetPassword { get; }
		public string PathVerifyEmail { get; }

		public FrontendSettings(IOptions<CustomOptions> customOptions)
		{
			BaseUrl = customOptions.Value.FRONTEND_BASE_URL;
			PathResetPassword = customOptions.Value.FRONTEND_PATH_RESET_PASSWORD;
			PathVerifyEmail = customOptions.Value.FRONTEND_PATH_VERIFY_EMAIL;
		}

		public Uri GetResetPasswordUri()
		{
			return new Uri($"{BaseUrl}/{PathResetPassword}");
		}
		
		public Uri GetVerifyEmailUri()
		{
			return new Uri($"{BaseUrl}/{PathVerifyEmail}");
		}
	}
}
