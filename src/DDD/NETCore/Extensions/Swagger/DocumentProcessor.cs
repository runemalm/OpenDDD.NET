using System;
using System.Linq;
using DDD.Application.Settings;
using DDD.Application.Settings.Http;
using DDD.Infrastructure.Ports.Adapters.Http.Common;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace DDD.NETCore.Extensions.Swagger
{
	public class DocumentProcessor : IDocumentProcessor
	{
		private ISettings _settings;
		private int _majorVersion;

		public DocumentProcessor(ISettings settings, int majorVersion)
		{
			_settings = settings;
			_majorVersion = majorVersion;
		}

		public void Process(DocumentProcessorContext context)
		{
			context.Document.Info.Version = GetVersion(context);
			context.Document.Info.Description =
				"The API follows the contract/expand strategy." +
				"This means the versioning policy is additive.<br>" +
				"Within the same major version, all endpoints are backwards compatible.<br>" +
				"Only the APIs with the latest patch version of each major version are defined below.";
		}
		
		private string GetVersion(DocumentProcessorContext context)
		{
			var controller = context.ControllerTypes.FirstOrDefault();
		
			if (controller == null)
				return "no versions";
			
			var attribute = 
				controller
					.GetCustomAttributesData()
					.First(a => a.AttributeType == typeof(VersionAttribute));
		
			if (attribute == null)
				throw new Exception(
					"HttpAdapter class is missing the 'Version' " +
					"attribute. Please add it.");
		
			var value = attribute.ConstructorArguments[0].Value;
			
			if (value == null)
			{
				if (attribute == null)
					throw new Exception(
						"HttpAdapter 'Version' attribute is missing " +
						"version string argument. Please add it.");
			}
			
			return value.ToString();
		}
	}
}
