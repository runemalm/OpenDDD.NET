using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDD.Application.Error;
using Microsoft.AspNetCore.Http;
using DDD.Logging;
using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Domain.Model.Auth.Exceptions;
using DDD.Domain.Model.Error;

namespace DDD.NETCore.Middleware
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
                    throw AuthorizeException.InvalidCredentials(
                        "There was more than one authorization token in the headers.");

                raw = tokens.FirstOrDefault()?.Substring(start.Length);
            } 
            else
            {
                throw new SettingsException(
                    ApplicationError.Settings_UnsupportedJwtTokenLocationSetting(_settings.Auth.JwtToken.Location));
            }

            if (raw != null)
            {
                // TODO: Read tokens from claims..
                var jwtToken = JwtToken.Read(raw, new List<string>());
                return jwtToken;
            }

            return null;
        }

        private IEnumerable<string> GetRolesClaimsTypesForAuthMethod(AuthMethod authMethod)
        {
            return new List<string>();
        }
    }
}
