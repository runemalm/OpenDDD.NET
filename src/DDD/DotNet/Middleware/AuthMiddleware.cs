using System;
using System.Linq;
using System.Threading.Tasks;
using DDD.Application.Settings.Exceptions;
using DDD.Domain.Auth;
using DDD.Domain.Auth.Exceptions;
using DDD.Logging;
using DDD.Application.Settings;
using Microsoft.AspNetCore.Http;

namespace DDD.DotNet.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISettings _settings;
        private readonly ILogger _logger;

        public AuthMiddleware(RequestDelegate next, ISettings settings, ILogger logger)
        {
            _next = next;
            _settings = settings;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ICredentials credentials)
        {
            credentials.JwtToken = null;

            if (_settings.Auth.Enabled)
			    credentials.JwtToken = GetJwtToken(context);

            await _next(context);
        }

        private JwtToken GetJwtToken(HttpContext context)
		{
            string raw;

            if (_settings.Auth.JwtToken.Location.ToLower() == "header")
			{
                var start = $"{_settings.Auth.JwtToken.Scheme} ";
                var authorizationHeaders = context.Request.Headers[_settings.Auth.JwtToken.Name];
                var tokens = authorizationHeaders.Where(ah => ah.StartsWith(start, StringComparison.OrdinalIgnoreCase));

                if (tokens.Count() > 1)
                    throw new InvalidCredentialsException(
                        $"There was more than one authorization " +
						$"token in the headers.");

                raw = tokens.FirstOrDefault()?.Substring(start.Length);
            } 
            else
			{
                throw new SettingsException(
                    $"Unsupported jwt token location setting: {_settings.Auth.JwtToken.Location}");
			}

            if (raw != null)
                return JwtToken.Read(raw);

            return null;
        }
    }
}
