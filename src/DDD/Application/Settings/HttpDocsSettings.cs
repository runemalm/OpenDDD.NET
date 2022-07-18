using System;
using System.Collections.Generic;
using DDD.DotNet.Extensions;
using DDD.Application.Settings.Exceptions;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings
{
	public class HttpDocsSettings : IHttpDocsSettings
	{
		public IEnumerable<HttpDocsDefinition> Definitions { get; }
		public IEnumerable<HttpDocsAuthExtraToken> AuthExtraTokens { get; }
		public bool Enabled { get; }
		public bool HttpEnabled { get; }
		public bool HttpsEnabled { get; }
		public string Hostname { get; }
		public string Title { get; }

		public HttpDocsSettings() { }

		public HttpDocsSettings(IOptions<Options> options)
		{
			var enabled = options.Value.HTTP_DOCS_ENABLED.IsTrue();
			var httpEnabled = options.Value.HTTP_DOCS_HTTP_ENABLED.IsTrue();
			var httpsEnabled = options.Value.HTTP_DOCS_HTTPS_ENABLED.IsTrue();
			var hostname = options.Value.HTTP_DOCS_HOSTNAME;
			var title = options.Value.HTTP_DOCS_TITLE;

			// Defs
			var definitions = new List<HttpDocsDefinition>();
			var chunks =
				options.Value.HTTP_DOCS_DEFINITIONS?.Split(
					new string[] { "::" }, StringSplitOptions.None) ?? new string[0];

			foreach (var chunk in chunks)
			{
				try
				{
					var def = new HttpDocsDefinition(chunk);
					definitions.Add(def);
				}
				catch (Exception e)
				{
					throw new SettingsException(
						$"Couldn't parse http docs definition selector from " +
						$"setting string: {chunk}", e);
				}
			}

			// Extra auth tokens
			var extraTokens = new List<HttpDocsAuthExtraToken>();
			chunks = new string[0];

			if (options.Value.HTTP_DOCS_AUTH_EXTRA_TOKENS != null)
			{
				chunks =
					options.Value.HTTP_DOCS_AUTH_EXTRA_TOKENS.Split(
						new string[] { "::" }, StringSplitOptions.None);
			}

			foreach (var chunk in chunks)
			{
				try
				{
					var token = new HttpDocsAuthExtraToken(chunk);
					extraTokens.Add(token);
				}
				catch (Exception e)
				{
					throw new SettingsException(
						$"Couldn't parse http docs auth extra token " +
						$"from setting string: {chunk}", e);
				}
			}

			Enabled = enabled;
			Definitions = definitions;
			AuthExtraTokens = extraTokens;
			Enabled = enabled;
			HttpEnabled = httpEnabled;
			HttpsEnabled = httpsEnabled;
			Hostname = hostname;
			Title = title;
		}
	}
}
