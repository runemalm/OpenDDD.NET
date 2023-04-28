using System;

namespace OpenDDD.Application.Settings.Http
{
	public class HttpDocsAuthExtraToken
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public string KeyName { get; set; }
		public string Description { get; set; }

		public HttpDocsAuthExtraToken() { }

		public HttpDocsAuthExtraToken(string name, string location, string keyName, string description)
		{
			Name = name;
			Location = location;
			KeyName = keyName;
			Description = description;
		}

		public HttpDocsAuthExtraToken(string settingString)
		{
			Name = settingString;

			var chunks = settingString.Split(',');

			if (chunks.Length != 4)
				throw new Exception(
					"Http docs auth extra token string must be " +
					"in the format: 'name,location,keyName,description'.");

			Name = chunks[0];
			Location = chunks[1];
			KeyName = chunks[2];
			Description = chunks[3];
		}
	}
}
