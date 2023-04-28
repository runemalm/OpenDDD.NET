﻿using System;

namespace OpenDDD.Application.Settings.Http
{
	public class HttpDocsDefinition
	{
		public string Name { get; set; }
		public string Attribute { get; set; }
		public string BasePath { get; set; }

		public HttpDocsDefinition(string settingString)
		{
			Name = settingString;

			var chunks = settingString.Split(',');

			if (chunks.Length != 3)
				throw new Exception(
					"Http docs definition setting string must be " +
					"in the format: 'name,attribute,basePath'.");

			Name = chunks[0];
			Attribute = chunks[1];
			BasePath = chunks[2];
		}
	}
}