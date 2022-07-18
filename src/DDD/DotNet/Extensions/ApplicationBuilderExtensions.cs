using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DDD.Application;
using DDD.Infrastructure.Converters;
using DDD.Domain.Exceptions;
using DDD.Application.Exceptions;
using DDD.Infrastructure.Ports;
using DDD.DotNet.Middleware;
using DDD.Application.Settings;
using DDD.Infrastructure.Converters.NewtonSoft;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.DotNet.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		// Public API

		public static IApplicationBuilder AddDdd(this IApplicationBuilder app, ISettings settings, IApplicationLifetime applicationLifetime)
		{
			app.AddAccessControl(settings);
			app.AddHttpAdapter(settings);
			app.AddTranslation(settings);
			app.StartContext();
			
			// applicationLifetime.ApplicationStopping.Register(OnShutdown, app);
			
			return app;
		}

		// private static void OnShutdown()
		// {
		// 	StopContext();
		// }

		public static IApplicationBuilder AddHttpAdapter(this IApplicationBuilder app, ISettings settings)
		{
			app.AddHttpAdapterErrorFormatting(settings);
			app.AddHttpAdapterDocs(settings);
			app.AddCorsPolicy(settings);
			return app;
		}
		
		public static IApplicationBuilder AddTranslation(this IApplicationBuilder app, ISettings settings)
		{
			app.AddJsonConverterPolicy(settings);
			return app;
		}

		public static IApplicationBuilder StartContext(this IApplicationBuilder app)
		{
			app.StartSecondaryAdapters();
			app.StartPrimaryAdapters();
			return app;
		}
		
		// Private API

		private static IApplicationBuilder AddCorsPolicy(this IApplicationBuilder app, ISettings settings)
		{
			app.UseCors();
			return app;
		}

		private static IApplicationBuilder AddJsonConverterPolicy(this IApplicationBuilder app, ISettings settings)
		{
			// System.Text.Json
			
			// This is a work-around to be able to set default serializer options
			// See: https://stackoverflow.com/a/58959198
			var opts = ((JsonSerializerOptions)typeof(JsonSerializerOptions)
				.GetField("s_defaultOptions",
					System.Reflection.BindingFlags.Static |
					System.Reflection.BindingFlags.NonPublic)
				?.GetValue(null));

			if (opts != null)
			{
				// Don't do it twice, because then an exception will be thrown.
				if (opts.Converters.Count == 0)
				{
					opts.PropertyNameCaseInsensitive = true;
					opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
					opts.Converters.Add(new JsonStringEnumConverter());
					opts.Converters.Add(new ActionIdConverter());
					opts.Converters.Add(new EventIdConverter());
					opts.Converters.Add(new EntityIdConverter());
					opts.Converters.Add(new DomainModelVersionConverter());
				}
			}
			else
			{
				throw new DddException("Couldn't configure json converters.");
			}

			// Newtonsoft
			
			// We use Newtonsoft for OutboxEvents serialization only.
			// This is because System.Text.Json (that we use everywhere else),
			// doesn't support polymorphic serialization.
			//
			// The goal is to use System.Text.Json everywhere in the future.

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Converters = new List<JsonConverter>()
				{
					new StringEnumConverter(),
					new ActionIdNewtonsoftConverter(),
					new EventIdNewtonsoftConverter(),
					new EntityIdNewtonsoftConverter(),
					new DomainModelVersionNewtonsoftConverter()
				}
			};
			
			return app;
		}

		private static IApplicationBuilder AddAccessControl(this IApplicationBuilder app, ISettings settings)
		{
			app.UseMiddleware<AuthMiddleware>(settings);
			return app;
		}

		private static IApplicationBuilder AddHttpAdapterDocs(this IApplicationBuilder app, ISettings settings)
		{
			if (settings.Http.Docs.Enabled)
				app.AddNswagMiddleware(settings);

			return app;
		}

		private static IApplicationBuilder AddHttpAdapterErrorFormatting(this IApplicationBuilder app, ISettings settings)
		{
			// TODO: Refactor this..
			// TODO: Add error codes..

			app.UseExceptionHandler(
                c => c.Run(async context =>
                {
                    var exception = context.Features
                        .Get<IExceptionHandlerPathFeature>()
                        .Error;

					// Check if not found
					var isNotFoundException = false;

					var toCheck = exception.GetType();
					var baseType = typeof(EntityNotFoundException<>);

					while (toCheck != typeof(object))
					{
						Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
						if (baseType == cur)
						{
							isNotFoundException = true;
						}

						toCheck = toCheck.BaseType;
					}
					
					// Check if invalid command exception
					var isInvalidCommandException = false;

					toCheck = exception.GetType();
					baseType = typeof(InvalidCommandException);

					while (toCheck != typeof(object))
					{
						Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
						if (baseType == cur)
						{
							isInvalidCommandException = true;
						}

						toCheck = toCheck.BaseType;
					}

					// Prepare response
					if (!context.Response.HasStarted)
						context.Response.Clear();

					context.Response.ContentType = "application/json";

					// Add error json
					Failure failureResponse =
						new Failure(
							new List<Error>()
							{
								new Error("TODO", exception.Message)
							});

					if (isNotFoundException)
						context.Response.StatusCode = StatusCodes.Status404NotFound;
					else if (isInvalidCommandException)
						context.Response.StatusCode = StatusCodes.Status400BadRequest;
					else
						context.Response.StatusCode = StatusCodes.Status500InternalServerError;

					// Respond
					await context.Response.WriteAsync(JsonSerializer.Serialize(failureResponse));

				}));

            return app;
		}

		// private static void StopContext(IServiceCollection services)
		// {
		// 	// TODO: Stop adapters..
		// 	app.StopPrimaryAdapters();
		// 	app.StopSecondaryAdapters();
		// 	return app;
		// }

		private static IApplicationBuilder AddNswagMiddleware(this IApplicationBuilder app, ISettings settings)
		{

			app
				.UseOpenApi(configure =>
				{
					configure.PostProcess = (document, request) =>
					{
						document.Schemes = new NSwag.OpenApiSchema[] { };

						if (settings.Http.Docs.HttpEnabled)
							document.Schemes.Add(NSwag.OpenApiSchema.Http);

						if (settings.Http.Docs.HttpsEnabled)
							document.Schemes.Add(NSwag.OpenApiSchema.Https);

						if (settings.Http.Docs.Hostname != "" && settings.Http.Docs.Hostname != null)
							document.Host = settings.Http.Docs.Hostname;
					};
				})
				.UseSwaggerUi3(c =>
				{
					c.DocExpansion = "list";
				});
			return app;
		}

		private static IApplicationBuilder StartPrimaryAdapters(this IApplicationBuilder app)
		{
			app.StartListeners();
			return app;
		}

		private static IApplicationBuilder StartSecondaryAdapters(this IApplicationBuilder app)
		{
			app.StartInterchangeEventAdapter();
			app.StartDomainEventAdapter();
			// app.StartRepositories();
			return app;
		}

		private static IApplicationBuilder StartListeners(this IApplicationBuilder app)
		{
			foreach (var listener in app.ApplicationServices.GetServices<IEventListener>())
				listener.Start().Wait();

			return app;
		}

		private static IApplicationBuilder StartInterchangeEventAdapter(this IApplicationBuilder app)
		{
			var interchangeEventAdapter =
				app.ApplicationServices.GetService<IInterchangeEventAdapter>();

			if (interchangeEventAdapter != null)
				interchangeEventAdapter.StartAsync().Wait();

			return app;
		}

		private static IApplicationBuilder StartDomainEventAdapter(this IApplicationBuilder app)
		{
			var domainEventAdapter =
				app.ApplicationServices.GetService<IDomainEventAdapter>();

			if (domainEventAdapter != null)
				domainEventAdapter.StartAsync().Wait();

			return app;
		}
		
		// private static IApplicationBuilder StartRepositories(this IApplicationBuilder app)
		// {
			// var services = app.ApplicationServices.GetServices();
			//
			// var result = new Dictionary<Type, ServiceDescriptor>();
			//
			// var engine = app.ApplicationServices.G("_engine");
			// var callSiteFactory = engine.GetPropertyValue("CallSiteFactory");
			// var descriptorLookup = callSiteFactory.GetFieldValue("_descriptorLookup");
			// if (descriptorLookup is IDictionary dictionary)
			// {
			// 	foreach (DictionaryEntry entry in dictionary)
			// 	{
			// 		result.Add((Type)entry.Key, (ServiceDescriptor)entry.Value.GetPropertyValue("Last"));
			// 	}
			// }
			//
			// return result;
			
			// var repositories = app.ApplicationServices.GetALlS
			
			
			// var repositories = app.ApplicationServices.GetServices<RabbitSettings>();
			// // app.ApplicationServices.GetServices<IStartupTask>()
			//
			// foreach (var repository in repositories)
			// 	Console.WriteLine(repository);
			//
			// return app;
		// }
	}
}
