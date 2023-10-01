using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.Adapters.Database.Memory;
using OpenDDD.Infrastructure.Ports.Events;
using OpenDDD.Infrastructure.Services.Serialization;
using OpenDDD.Main;
using OpenDDD.NET.Extensions.Swagger;
using OpenDDD.NET.Services.DatabaseConnection;
using OpenDDD.NET.Services.DatabaseConnection.Memory;
using OpenDDD.NET.Services.Outbox;

namespace OpenDDD.NET.Extensions
{
	public static class ServiceCollectionExtensions
	{
		// Public Convenience Group Methods
		
		public static IServiceCollection AddSerializer<TSettingsImplementation>(this IServiceCollection services)
			where TSettingsImplementation : class, ISerializerSettings
		{
			services.AddSerializerSettings<TSettingsImplementation>();
			services.AddTransient<ISerializer, Serializer>();
			return services;
		}
		
		public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
		{
			services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
			return services;
		}
		
		public static IServiceCollection AddHttpTranslation<TImplementation>(this IServiceCollection services)
			where TImplementation : class
		{
			services.AddTransient<TImplementation>();
			return services;
		}

		public static IServiceCollection AddMemoryDatabase(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton<IMemoryDatabaseStore, MemoryDatabaseStore>();
			services.AddSingleton<IMemoryDatabase, MemoryDatabase>();
			return services;
		}
		
		public static IServiceCollection AddEventProcessor(this IServiceCollection services, IConfiguration configuration)
		{
			// services.AddEventProcessorDatabaseConnection(Configuration);
			// services.AddEventProcessorMessageBrokerConnection(Configuration);
			// services.AddEventProcessorOutbox(Configuration);
			// services.AddHostedService<IEventProcessorService, ...>();
			
			return services;
		}
		
		public static IServiceCollection MaybeAddDatabase(this IServiceCollection services, IConfiguration configuration)
		{
			if (configuration["ActionDatabaseConnection:Provider"].ToLower() == "memory")
			{
				services.AddMemoryDatabase(configuration);
			}
			
			return services;
		}

		public static IServiceCollection AddActionDatabaseConnection(this IServiceCollection services, IConfiguration configuration)
		{
			string provider = configuration["ActionDatabaseConnection:Provider"].ToLower();
			
			if (provider == "memory")
			{
				services.AddSettings<IMemoryActionDatabaseConnectionSettings, MemoryActionDatabaseConnectionSettings>(configuration, "ActionDatabaseConnection.Memory");
				services.AddScoped<IActionDatabaseConnection, MemoryActionDatabaseConnection>();
			}
			else if (provider == "postgres")
			{
				// services.Configure<PostgresTransactionalDbConnectionSettings>(configuration["TransactionalConnection.Postgres"]);
				// services.AddScoped<ITransactionalDbConnection, PostgresDbConnection<PostgresTransactionalDbConnectionSettings>>();
			}
			
			return services;
		}
		
		public static IServiceCollection AddActionOutbox(this IServiceCollection services)
		{
			services.AddTransient<IActionOutbox, ActionOutbox>();
			return services;
		}
		
		public static IServiceCollection AddDomainPublisher(this IServiceCollection services)
		{
			services.AddTransient<IDomainPublisher, DomainPublisher>();
			return services;
		}

		public static IServiceCollection AddSettings<TInterface, TImplementation>(this IServiceCollection services, IConfiguration configuration, string sectionName)
			where TInterface : class, ISettings
			where TImplementation : class, TInterface, new()
		{
			// Bind the MyAppSettings section to your settings class
			services.Configure<TImplementation>(configuration.GetSection(sectionName));

			// Register the interface and resolve it to the settings class
			services.AddScoped<TInterface, TImplementation>();

			return services;
		}
		
		public static IServiceCollection AddRepository<TInterface, TImplementation>(this IServiceCollection services)
			where TImplementation : class, TInterface
		{
			services.AddTransient(typeof(TInterface), typeof(TImplementation));
			return services;
		}

		public static IServiceCollection AddAction<TAction, TCommand>(this IServiceCollection services)
		{
			services.AddTransient(typeof(TAction));
			services.AddTransient(typeof(TCommand));
			return services;
		}
		
		public static IServiceCollection AddEnsureDataTask<TImplementation>(this IServiceCollection services)
			where TImplementation : class, IEnsureDataTask
		{
			services.AddTransient(typeof(IEnsureDataTask), typeof(TImplementation));
			return services;
		}
		
		// Private
		
		private static IServiceCollection AddSerializerSettings<TSettingsImplementation>(this IServiceCollection services)
			where TSettingsImplementation : class, ISerializerSettings
		{
			services.AddTransient<ISerializerSettings, TSettingsImplementation>();
			return services;
		}

		public static IServiceCollection AddSwaggerDocuments(this IServiceCollection services, IEnumerable<int> majorVersions, ISerializer serializer)
		{
			/*
			 * Each 'document' corresponds to a specific api version definition in the UI.
			 * Documents are added by the NSwag 'generator'.
			 * 
			 * The generator is defined below, it adds 'processors'.
			 * The 'document processor' is used to add the 'security definitions'.
			 * The 'operation processor' is used to add the endpoints.
			 *
			 * So NSwag will used the configured generator below to create the openapi yml file.
			 */
			foreach (var majorVersion in majorVersions)
			{
				services.AddSwaggerDocument(majorVersion, "", "", serializer);
			}

			return services;
		}
		
		// Private
		
		private static IServiceCollection AddSwaggerDocument(this IServiceCollection services, int majorVersion, string definitionName, string basePath, ISerializer serializer)
		{
			services.AddOpenApiDocument(s =>
			{
				s.Title = "ThMap API";
				s.DocumentName = $"API Major Version {majorVersion}{(!definitionName.IsNullOrEmpty() ? " ("+definitionName+")" : "")}";
				s.DocumentProcessors.Add(new DocumentProcessor());
				s.SerializerSettings = serializer.Settings.JsonSerializerSettings;

				// Security definitions
				var securityNames = new List<string>();

				s.DocumentProcessors.Add(
					new SecurityDefinitionAppender(
						"JWT Token",
						new OpenApiSecurityScheme
						{
							Type = OpenApiSecuritySchemeType.ApiKey,
							Name = "Authorization",
							In = OpenApiSecurityApiKeyLocation.Header,
							Description =
								$"Type into the textbox: " +
								$"Bearer " +
								$"{{your jwt token}}."
						}));

				securityNames.Add("JWT Token");

				// Security requirements
				s.OperationProcessors.Insert(
					0,
					new OperationProcessor(
						majorVersion,
						definitionName,
						securityNames,
						basePath,
						"localhost:5001",
						true,
						true,
						80,
						443));
			});
			
			return services;
		}
	}
}
