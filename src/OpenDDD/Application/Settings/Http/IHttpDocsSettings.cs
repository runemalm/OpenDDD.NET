using System.Collections.Generic;

namespace OpenDDD.Application.Settings.Http
{
	public interface IHttpDocsSettings
	{
		IEnumerable<int> MajorVersions { get; }
		IEnumerable<HttpDocsDefinition> Definitions { get; }
		IEnumerable<HttpDocsAuthExtraToken> AuthExtraTokens { get; }
		bool Enabled { get; }
		bool HttpEnabled { get; }
		bool HttpsEnabled { get; }
		string Hostname { get; }
		int HttpPort { get; }
		int HttpsPort { get; }
		string Title { get; }
	}
}
