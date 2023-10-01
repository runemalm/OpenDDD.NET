using System;
using System.Linq;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using OpenDDD.Infrastructure.Ports.Adapters.Http.Common;

namespace OpenDDD.NET.Extensions.Swagger
{
	public class DocumentProcessor : IDocumentProcessor
	{
		public DocumentProcessor()
		{
			
		}

		public void Process(DocumentProcessorContext context)
		{
			context.Document.Info.Version = GetVersion(context);
			context.Document.Info.Description =
				"The API provides endpoints for searching and retrieving available properties for rent in Thailand.<br>" +
				"SemVer2.0 versioning policy is in effect.";
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
