using System.Collections.Generic;

namespace DDD.Application.Settings.Http
{
	public interface IHttpDocsSettings
	{
		IEnumerable<HttpDocsDefinition> Definitions { get; }
		IEnumerable<HttpDocsAuthExtraToken> AuthExtraTokens { get; }
		bool Enabled { get; }
		bool HttpEnabled { get; }
		bool HttpsEnabled { get; }
		string Hostname { get; }
		string Title { get; }
	}
}
