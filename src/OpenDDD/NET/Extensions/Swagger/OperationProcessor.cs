using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using OpenDDD.Domain.Model.Auth;
using OpenDDD.Infrastructure.Ports.Adapters.Http.Common;
using OpenDDD.Infrastructure.Ports.Adapters.Http.NETCore;

namespace OpenDDD.NET.Extensions.Swagger
{
	public class OperationProcessor : IOperationProcessor
	{
		private readonly int _majorVersion;
		private readonly string _defAttributeName;
		private readonly IEnumerable<string> _securityNames;
		private readonly string _basePath;
		private readonly string _hostname;
		private readonly bool _httpEnabled;
		private readonly bool _httpsEnabled;
		private readonly int _httpPort;
		private readonly int _httpsPort;

		public OperationProcessor(
			int majorVersion,
			string defAttributeName,
			IEnumerable<string> securityNames,
			string basePath,
			string hostname,
			bool httpEnabled,
			bool httpsEnabled,
			int httpPort,
			int httpsPort)
		{
			_majorVersion = majorVersion;
			_defAttributeName = defAttributeName;
			_securityNames = securityNames;
			_basePath = basePath;
			_hostname = hostname;
			_httpEnabled = httpEnabled;
			_httpsEnabled = httpsEnabled;
			_httpPort = httpPort;
			_httpsPort = httpsPort;
		}

		public bool Process(OperationProcessorContext context)
		{
			var isFromHttpAdapter = context.ControllerType.IsSubclassOf(typeof(NETCoreHttpAdapter));

			var isCorrectVersion = GetMajorVersion(context) == _majorVersion;

			var passAttributeFilter = true;

			if (!_defAttributeName.IsNullOrEmpty())
			{
				passAttributeFilter =
					context.MethodInfo.GetCustomAttributes().Any(
						a =>
							a.GetType().IsSubclassOf(typeof(DocsDefinitionAttribute)) &&
							a.GetType().Name == _defAttributeName);
			}

			if (isFromHttpAdapter)
			{
				context.OperationDescription.Operation.IsDeprecated = GetDeprecationStatus(context);
				context.OperationDescription.Operation.OperationId = context.MethodInfo.Name;
			}

			// Protected
			var protectedAttributes =
				context.MethodInfo.DeclaringType.GetCustomAttributes(true)
				.Union(context.MethodInfo.GetCustomAttributes(true))
				.OfType<ProtectedAttribute>();

			var allowAnonymousAttributes =
				context.MethodInfo.DeclaringType.GetCustomAttributes(true)
				.Union(context.MethodInfo.GetCustomAttributes(true))
				.OfType<AllowAnonymousAttribute>();
			
			// Section
			var sectionAttributes =
				context.MethodInfo.DeclaringType.GetCustomAttributes(true)
					.Union(context.MethodInfo.GetCustomAttributes(true))
					.OfType<SectionAttribute>();

			var hasSection = sectionAttributes.Any();
			var hasProtected = protectedAttributes.Any();
			var hasAllowAnonymous = allowAnonymousAttributes.Any();
			var isProtected = hasProtected && !hasAllowAnonymous;
			
			context.OperationDescription.Operation.Tags = new List<string>();
			
			if (hasSection)
				context.OperationDescription.Operation.Tags.Add(sectionAttributes.First().Name);
			else
				context.OperationDescription.Operation.Tags.Add("Others");

			if (isProtected)
			{
				context.OperationDescription.Operation.Responses.Add(
					"401", new OpenApiResponse { Description = "Unauthorized - there was something wrong with your credentials." });
				context.OperationDescription.Operation.Responses.Add(
					"403", new OpenApiResponse { Description = "Forbidden - you don't have enough permissions to execute the action." });

				var schemes = new OpenApiSecurityRequirement();

				foreach (var securityName in _securityNames)
					schemes.Add(securityName, new List<string>());

				context.OperationDescription.Operation.Security =
					new List<OpenApiSecurityRequirement> { schemes };
			}

			// Add common responses to scheme
			context.OperationDescription.Operation.Responses.Add(
				"400", new OpenApiResponse { Description = "Bad Request - you sent invalid data." });
			context.OperationDescription.Operation.Responses.Add(
				"404", new OpenApiResponse { Description = "Not Found - one or more entities could not be found." });
			context.OperationDescription.Operation.Responses.Add(
				"500", new OpenApiResponse { Description = "Internal Server Error - an unknown error occured." });

			// Base path
			if (_basePath != "")
			{
				var servers = new List<OpenApiServer>();
				
				if (_httpEnabled)
					servers.Add(new OpenApiServer
						{
							Url = $"http://{_hostname}{(_httpPort != 80 ? ":"+_httpPort : "")}/{_basePath}"
						});
				
				if (_httpsEnabled)
					servers.Add(new OpenApiServer
					{
						Url = $"https://{_hostname}{(_httpsPort != 443 ? ":"+_httpsPort : "")}/{_basePath}"
					});
				
				context.OperationDescription.Operation.Servers = servers;
			}

			return isFromHttpAdapter && isCorrectVersion && passAttributeFilter;
		}

		private bool GetDeprecationStatus(OperationProcessorContext context)
		{
			var versionDeprecated = IsVersionDeprecated(context);
			var endpointDeprecated = IsEndpointDeprecated(context);

			return endpointDeprecated || versionDeprecated;
		}

		private bool IsVersionDeprecated(OperationProcessorContext context)
		{
			var hasAttribute =
				context.ControllerType.GetCustomAttributesData().Any(
					a => a.AttributeType == typeof(DeprecatedAttribute));

			return hasAttribute;
		}

		private bool IsEndpointDeprecated(OperationProcessorContext context)
		{
			var hasAttribute =
				context.MethodInfo.GetCustomAttributes().Any(
					a => a.GetType() == typeof(DeprecatedAttribute));

			return hasAttribute;
		}

		private string GetVersion(OperationProcessorContext context)
		{
			var attribute = 
				context.ControllerType.GetCustomAttributesData()
				.First(a => a.AttributeType == typeof(VersionAttribute));

			if (attribute == null)
				throw new Exception(
					"HttpAdapter class is missing the 'Version' " +
					"attribute. Please add it.");

			var version = attribute.ConstructorArguments[0].Value.ToString();
			
			version = version.Replace("v", "");

			return version;
		}
		
		private string GetWildcardVersion(OperationProcessorContext context)
		{
			var version = GetVersion(context);
			var chunks = version.Split('.');
			var wildcardVersion = $"{chunks.First()}.{string.Join('.', chunks.Skip(1).Select(n => "x"))}";
			return wildcardVersion;
		}
		
		private int GetMajorVersion(OperationProcessorContext context)
		{
			var version = GetVersion(context);
			var majorVersion = Int32.Parse(version.Split('.').First());
			return majorVersion;
		}

		// private ICollection<string> OrderVersions(ICollection<string> versions)
		// {
		// 	var ordered = new List<Version>();
		// 	var hasV1 = false;
		//
		// 	foreach (var s in versions)
		// 	{
		// 		Version semantic = null;
		//
		// 		if (s == "v1")
		// 			hasV1 = true;   // Exception for v1
		// 		else
		// 		{
		// 			try
		// 			{
		// 				semantic = new Version(s.Replace("v", ""));
		// 			}
		// 			catch (Exception)
		// 			{
		// 				throw new Exception(
		// 					$"You must use semantic API versions " +
		// 					$"in your http adapters (vx.y.z). " +
		// 					$"Invalid version: {s}");
		// 			}
		//
		// 			ordered.Add(semantic);
		// 		}
		// 	}
		//
		// 	ordered.Sort();
		// 	ordered.Reverse();
		//
		// 	var result = ordered.Select(o => $"v{o}").ToList();
		//
		// 	if (hasV1)
		// 		result.Insert(ordered.Count(), "v1");
		//
		// 	return result;
		// }

		// private string FirstCharToUpper(string input)
		// {
		// 	switch (input)
		// 	{
		// 		case null: throw new ArgumentNullException(nameof(input));
		// 		case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
		// 		default: return input[0].ToString().ToUpper() + input.Substring(1);
		// 	}
		// }
	}
}
