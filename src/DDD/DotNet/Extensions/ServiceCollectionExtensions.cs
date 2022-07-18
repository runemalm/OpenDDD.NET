using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DDD.Domain;
using DDD.Domain.Auth;
using DDD.Application.Exceptions;
using DDD.Application.Settings.Exceptions;
using DDD.Infrastructure.Persistence;
using DDD.Infrastructure.Persistence.Memory;
using DDD.Infrastructure.Persistence.Postgres;
using DDD.Infrastructure.Ports;
using DDD.Infrastructure.Ports.Adapters.AppInsights;
using DDD.Infrastructure.Ports.Adapters.DotNet;
using DDD.Infrastructure.Ports.Adapters.Http;
using DDD.Infrastructure.Ports.Adapters.Memory;
using DDD.Infrastructure.Ports.Adapters.Rabbit;
using DDD.Infrastructure.Ports.Adapters.Repositories.Memory;
using DDD.Infrastructure.Ports.Adapters.Repositories.Postgres;
using DDD.Infrastructure.Ports.Adapters.ServiceBus;
using DDD.Infrastructure.Ports.Adapters.Smtp;
using DDD.Application.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NSwag.Generation.Processors.Security;

namespace DDD.DotNet.Extensions
{
	public static class ServiceCollectionExtensions
	{
		// Public API

		// public List<Type> RegisteredRepositoryTypes = new List<Type>();

		public static IServiceCollection AddDdd(this IServiceCollection services, ISettings settings, IDomainModelVersion domainModelVersion)
		{
			services.AddAccessControl(settings);
			services.AddEmail(settings);
			services.AddHttpAdapter(settings);
			services.AddHttpClient();
			services.AddMonitoring(settings);
			services.AddPersistence(settings);
			services.AddPubSub(settings);
			services.SetDomainModelVersion(domainModelVersion);
			return services;
		}
		
		public static IServiceCollection AddListener<TImplementation>(this IServiceCollection services)
			where TImplementation : class, IEventListener
		{
			// TODO: Don't register twice. We do it now as work-around to be
			//		 able to get all services registered as IEventListener,
			//		 and to get each individual service as XXXEventListener.
			services.AddTransient(typeof(IEventListener), typeof(TImplementation));
			services.AddTransient<TImplementation>();
			return services;
		}
		
		public static IServiceCollection AddAccessControl(this IServiceCollection services, ISettings settings)
		{
			services.AddSingleton<ICredentials, Credentials>();
			services.AddTransient<IAuthDomainService, AuthDomainService>();
			return services;
		}
		
		public static IServiceCollection AddEmail(this IServiceCollection services, ISettings settings)
		{
			if (settings.Email.Provider == EmailProvider.Smtp)
			{
				services.AddTransient<IEmailPort, SmtpEmailAdapter>();
			}
			else if (settings.Email.Provider == EmailProvider.Memory)
			{
				services.AddTransient<IEmailPort, MemoryEmailAdapter>();
			}
			else
			{
				throw new DddException(
					$"Can't add email for unsupported " +
					$"email provider: '{settings.Email.Provider}'.");
			}
			return services;
		}
		
		public static IServiceCollection AddHttpAdapter(this IServiceCollection services, ISettings settings)
		{
			services.AddHttpAdapterDocs(settings);
			services.AddCorsPolicy(settings);
			return services;
		}
		
		public static IServiceCollection AddMonitoring(this IServiceCollection services, ISettings settings)
		{
			if (settings.Monitoring.Provider == MonitoringProvider.Memory)
				services.AddSingleton<IMonitoringPort, MemoryMonitoringAdapter>();
			else if (settings.Monitoring.Provider == MonitoringProvider.AppInsights)
				services.AddSingleton<IMonitoringPort, AppInsightsMonitoringAdapter>();
			else
				throw new DddException(
					$"Can't add monitoring for unsupported provider: '{settings.Monitoring.Provider}'.");

			return services;
		}
		
		public static IServiceCollection AddPersistence(this IServiceCollection services, ISettings settings)
		{
			if (settings.Persistence.Provider == PersistenceProvider.Memory)
			{
				services.AddSingleton<IPersistenceService, MemoryPersistenceService>();
			}
			else if (settings.Persistence.Provider == PersistenceProvider.Postgres)
			{
				services.AddSingleton<IPersistenceService, PostgresPersistenceService>();
			}
			else
			{
				throw new DddException(
					$"Can't add persistence for unsupported " +
					$"persistence provider: '{settings.Persistence.Provider}'.");
			}
			return services;
		}

		public static IServiceCollection AddPubSub(this IServiceCollection services, ISettings settings)
		{
			services.AddPublishers(settings);
			services.AddEventAdapters(settings);
			services.AddEventRepository(settings);
			return services;
		}

		// TODO: Remove and use DomainModelVersion.Latest() everywhere instead.
		public static IServiceCollection SetDomainModelVersion(this IServiceCollection services, IDomainModelVersion domainModelVersion)
		{
			services.AddSingleton<IDomainModelVersion>(domainModelVersion);
			services.AddSingleton<DomainModelVersion>((DomainModelVersion)domainModelVersion);
			return services;
		}

		// public static IServiceCollection SetDomainModelVersion(this IServiceCollection services, int major, int minor, int build)
		// 	=> services.AddSingleton<IDomainModelVersion>(DomainModelVersion.Create(major, minor, build));
		
		public static IServiceCollection AddRepository<TPort, TAdapter>(this IServiceCollection services)
			where TAdapter : class
		{
			// var tracker = services.Get
			// RegisteredRepositoryTypes.Add(typeof(TPort));
			return services.AddSingleton(typeof(TPort), typeof(TAdapter));
			// return services.AddTransient(typeof(IEventListener), typeof(TImplementation));
		}

		// Private API

		private static IServiceCollection AddCorsPolicy(this IServiceCollection services, ISettings settings)
		{
			services.AddCors(options =>
			{
				options.AddDefaultPolicy(
					policy => { policy
						.WithOrigins(settings.Http.Cors.AllowedOrigins.ToArray())
						.AllowAnyHeader().WithMethods("GET", "POST", "DELETE");
					});
			});
			return services;
		}

		private static IServiceCollection AddHttpAdapterDocs(this IServiceCollection services, ISettings settings)
		{
			if (settings.Http.Docs.Enabled)
			{
				services.AddMicrosoftApiExplorer();
				services.AddSwaggerDocuments(settings);
			}
			return services;
		}

		private static IServiceCollection AddPublishers(this IServiceCollection services, ISettings settings)
		{
			services.AddSingleton<IInterchangePublisher, InterchangePublisher>();
			services.AddSingleton<IDomainPublisher, DomainPublisher>();
			return services;
		}

		private static IServiceCollection AddEventAdapters(this IServiceCollection services, ISettings settings)
		{
			services.AddInterchangeEventAdapter(settings);
			services.AddDomainEventAdapter(settings);
			return services;
		}
		
		private static IServiceCollection AddEventRepository(this IServiceCollection services, ISettings settings)
		{
			if (settings.Persistence.Provider == PersistenceProvider.Memory)
			{
				services.AddRepository<IEventRepository, MemoryEventRepository>();
			}
			else if (settings.Persistence.Provider == PersistenceProvider.Postgres)
			{
				services.AddRepository<IEventRepository, PostgresEventRepository>();
			}
			else
			{
				throw new DddException(
					$"Can't add event repository, unsupported persistence provider " +
					$"in config: '{settings.Persistence.Provider}'.");
			}
			return services;
		}

		private static IServiceCollection AddInterchangeEventAdapter(this IServiceCollection services, ISettings settings)
		{
			if (settings.PubSub.Provider == PubSubProvider.ServiceBus)
				services.AddSingleton<IInterchangeEventAdapter, ServiceBusInterchangeEventAdapter>();
			else if (settings.PubSub.Provider == PubSubProvider.Rabbit)
				services.AddSingleton<IInterchangeEventAdapter, RabbitInterchangeEventAdapter>();
			else if (settings.PubSub.Provider == PubSubProvider.Memory)
				services.AddSingleton<IInterchangeEventAdapter, MemoryInterchangeEventAdapter>();
			return services;
		}

		private static IServiceCollection AddDomainEventAdapter(this IServiceCollection services, ISettings settings)
		{
			if (settings.PubSub.Provider == PubSubProvider.ServiceBus)
				services.AddSingleton<IDomainEventAdapter, ServiceBusDomainEventAdapter>();
			else if (settings.PubSub.Provider == PubSubProvider.Rabbit)
				services.AddSingleton<IDomainEventAdapter, RabbitDomainEventAdapter>();
			else if (settings.PubSub.Provider == PubSubProvider.Memory)
				services.AddSingleton<IDomainEventAdapter, MemoryDomainEventAdapter>();
			return services;
		}

		private static IServiceCollection AddMicrosoftApiExplorer(this IServiceCollection services)
		{
			var builder = services.AddMvcCore();
			builder.AddApiExplorer();
			return services;
		}

		private static IServiceCollection AddSwaggerDocuments(this IServiceCollection services, ISettings settings)
		{
			foreach (var defSelector in settings.Http.Docs.Definitions)
			{
				var defAttributeName = defSelector.Attribute;

				services.AddOpenApiDocument(c =>
				{
					var title = $"{settings.General.Context} API";

					if (settings.Http.Docs.Title != "")
						title = settings.Http.Docs.Title;

					c.Title = title;
					c.DocumentName = defSelector.Name;
					c.DocumentProcessors.Add(new DocumentProcessor(settings));

					// Security definitions
					var securityNames = new List<string>();

					if (settings.Auth.Enabled)
					{
						services.ValidateJwtSettings(settings.Auth.JwtToken);
						
						c.DocumentProcessors.Add(
							new SecurityDefinitionAppender(
								"JWT Token",
								new OpenApiSecurityScheme
								{
									Type = OpenApiSecuritySchemeType.ApiKey,
									Name = settings.Auth.JwtToken.Name,
									In = ApiKeyLocationFromString(settings.Auth.JwtToken.Location),
									Description =
										$"Type into the textbox: " +
										$"{settings.Auth.JwtToken.Scheme} " +
										$"{{your jwt token}}."
								}));

						securityNames.Add("JWT Token");

						foreach (var extraToken in settings.Http.Docs.AuthExtraTokens)
						{
							c.DocumentProcessors.Add(
								new SecurityDefinitionAppender(
									extraToken.Name,
									new OpenApiSecurityScheme
									{
										Type = OpenApiSecuritySchemeType.ApiKey,
										Name = extraToken.KeyName,
										In = ApiKeyLocationFromString(extraToken.Location),
										Description = extraToken.Description
									}));

							securityNames.Add(extraToken.Name);
						}
					}

					// Security requirements
					c.OperationProcessors.Insert(
						0,
						new OperationProcessor(
							defAttributeName,
							securityNames,
							defSelector.BasePath,
							settings.Http.Docs.Hostname));
				});
			}
			return services;
		}
		
		private static IServiceCollection ValidateJwtSettings(this IServiceCollection services,
			IAuthJwtTokenSettings settings)
		{
			var errors = new List<string>();
			
			var allowedLocations = new List<string> { "header" };
			if (string.IsNullOrEmpty(settings.Location) || !allowedLocations.Contains(settings.Location.ToLower()))
				errors.Add($"'Location' must be one of: ('{string.Join("'|'", allowedLocations)}').");
			
			var allowedSchemes = new List<string> { "Bearer" };
			if (string.IsNullOrEmpty(settings.Scheme) || !allowedSchemes.Contains(settings.Scheme))
				errors.Add($"'Scheme' must be one of: ('{string.Join("'|'", allowedSchemes)}').");
			
			if (string.IsNullOrEmpty(settings.Name))
				errors.Add($"'Name' must be set.");

			if (errors.Count > 0)
				throw new SettingsException(
					$"Auth is enabled in the settings, but there are a/some invalid JWT auth setting(s). {string.Join(" ", errors)}");

			return services;
		}

		private static OpenApiSecurityApiKeyLocation ApiKeyLocationFromString(string value)
		{
			switch (value.ToLower())
			{
				case "cookie":
					return OpenApiSecurityApiKeyLocation.Cookie;
				case "header":
					return OpenApiSecurityApiKeyLocation.Header;
				case "query":
					return OpenApiSecurityApiKeyLocation.Query;
				default:
					throw new SettingsException(
						$"Unsupported 'location' in http docs auth def " +
						$"api key: {value}");
			}
		}

		// Helpers

		private class DocumentProcessor : IDocumentProcessor
		{
			private ISettings _settings;

			public DocumentProcessor(ISettings settings) : base()
			{
				_settings = settings;
			}

			public void Process(DocumentProcessorContext context)
			{
				context.Document.Info.Version = "all versions";
				context.Document.Info.Description =
					"These are the available domain actions.";
			}
		}

		private class OperationProcessor : IOperationProcessor
		{
			private ICollection<string> _versions = new List<string>();
			private string _defAttributeName;
			private IEnumerable<string> _securityNames;
			private string _basePath;
			private string _hostname;

			public OperationProcessor(
				string defAttributeName,
				IEnumerable<string> securityNames,
				string basePath,
				string hostname)
			{
				_defAttributeName = defAttributeName;
				_securityNames = securityNames;
				_basePath = basePath;
				_hostname = hostname;
			}

			public bool Process(OperationProcessorContext context)
			{
				var name = context.ControllerType.Name;

				// Is in base class and has docs definition attribute?
				var isFromHttpAdapter = context.ControllerType.IsSubclassOf(typeof(DotNetHttpAdapter));
				var passAttributeFilter =
					context.MethodInfo.GetCustomAttributes().Any(
						a =>
							a.GetType().IsSubclassOf(typeof(DocsDefinitionAttribute)) &&
							a.GetType().Name == _defAttributeName);

				// Version?
				var version = "";

				if (isFromHttpAdapter)
				{
					version = GetVersion(context);

					SaveVersion(version);

					context.OperationDescription.Operation.Tags = new List<string>() { version };
					context.OperationDescription.Operation.IsDeprecated = GetDeprecationStatus(context);
					context.OperationDescription.Operation.OperationId = context.MethodInfo.Name;
				}

				// Protected?
				var protectedAttributes =
					context.MethodInfo.DeclaringType.GetCustomAttributes(true)
					.Union(context.MethodInfo.GetCustomAttributes(true))
					.OfType<ProtectedAttribute>();

				var allowAnonymousAttributes =
					context.MethodInfo.DeclaringType.GetCustomAttributes(true)
					.Union(context.MethodInfo.GetCustomAttributes(true))
					.OfType<AllowAnonymousAttribute>();

				var hasProtected = protectedAttributes.Any();
				var hasAllowAnonymous = allowAnonymousAttributes.Any();
				var isProtected = hasProtected && !hasAllowAnonymous;

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
						new List<OpenApiSecurityRequirement>() { schemes };
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
					context.OperationDescription.Operation.Servers =
						new List<OpenApiServer>()
						{
							new OpenApiServer() { Url = $"{_hostname}/{_basePath}" }
						};

				return isFromHttpAdapter && passAttributeFilter;
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
					context.ControllerType.GetCustomAttributesData().Where(
						a => a.AttributeType == typeof(RouteAttribute)).First();

				if (attribute == null)
					throw new Exception(
						"HttpAdapter class is missing the 'Route' " +
						"attribute. Please add it.");

				var version = attribute.ConstructorArguments[0].Value.ToString();

				return version;
			}

			private void SaveVersion(string version)
			{
				if (!_versions.Contains(version)) _versions.Add(version);
				_versions = OrderVersions(_versions);
			}

			private ICollection<string> OrderVersions(ICollection<string> versions)
			{
				var ordered = new List<Version>();
				var hasV1 = false;

				foreach (var s in versions)
				{
					Version semantic = null;

					if (s == "v1")
						hasV1 = true;   // Exception for v1
					else
					{
						try
						{
							semantic = new Version(s.Replace("v", ""));
						}
						catch (Exception)
						{
							throw new Exception(
								$"You must use semantic API versions " +
								$"in your http adapters (vx.y.z). " +
								$"Invalid version: {s}");
						}

						ordered.Add(semantic);
					}
				}

				ordered.Sort();
				ordered.Reverse();

				var result = ordered.Select(o => $"v{o}").ToList();

				if (hasV1)
					result.Insert(ordered.Count(), "v1");

				return result;
			}

			private string FirstCharToUpper(string input)
			{
				switch (input)
				{
					case null: throw new ArgumentNullException(nameof(input));
					case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
					default: return input[0].ToString().ToUpper() + input.Substring(1);
				}
			}
		}
	}
}
