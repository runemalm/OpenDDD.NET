﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = System.Text.Json.JsonSerializer;
using DDD.Application;
using DDD.Application.Exceptions;
using DDD.Infrastructure.Ports;
using DDD.Application.Settings;
using DDD.Domain.Model.Auth.Exceptions;
using DDD.Domain.Model.BuildingBlocks.Entity;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.NewtonSoft;
using DDD.Infrastructure.Ports.PubSub;
using DDD.NETCore.Middleware;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace DDD.NETCore.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		// Public API

		public static IApplicationBuilder AddAccessControl(this IApplicationBuilder app, ISettings settings)
		{
			app.UseMiddleware<AuthMiddleware>(settings);
			return app;
		}
		
		public static IApplicationBuilder AddControl(this IApplicationBuilder app, IApplicationLifetime lifetime)
		{
			lifetime.ApplicationStarted.Register(app.OnAppStarted);
			lifetime.ApplicationStopping.Register(app.OnAppStopping);
			lifetime.ApplicationStopped.Register(app.OnAppStopped);
			return app;
		}

		public static IApplicationBuilder AddHttpAdapter(this IApplicationBuilder app, ISettings settings)
		{
			app
				.AddHttpAdapterErrorFormatting(settings)	// must be before UseRouting()..
				.UseRouting()
				.AddCorsPolicy(settings)
				.UseEndpoints(endpoints =>
				{
					endpoints.MapControllers();
				})
				.AddHttpAdapterDocs(settings);
			return app;
		}
		
		public static IApplicationBuilder AddTranslation(this IApplicationBuilder app, ISettings settings)
		{
			app.AddJsonConverterPolicy(settings);
			return app;
		}

		// Events

		public static void OnAppStarted(this IApplicationBuilder app)
			=> app.StartContext();

		public static void OnAppStopping(this IApplicationBuilder app)
			=> app.StopContext();

		public static void OnAppStopped(this IApplicationBuilder app)
		{
			
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

		private static IApplicationBuilder AddHttpAdapterDocs(this IApplicationBuilder app, ISettings settings)
		{
			if (settings.Http.Docs.Enabled)
				app.AddNswagMiddleware(settings);

			return app;
		}

		private static IApplicationBuilder AddHttpAdapterErrorFormatting(this IApplicationBuilder app, ISettings settings)
		{
			// TODO: Add error codes..

			app.UseExceptionHandler(
                c => c.Run(async context =>
                {
                    var exception = context.Features
                        .Get<IExceptionHandlerPathFeature>()
                        .Error;

                    var isNotFoundException = exception.IsOrIsSubType(typeof(EntityNotFoundException<>));
                    var isInvalidCommandException = exception.IsOrIsSubType(typeof(InvalidCommandException));
                    var isUnauthorizedException = 
	                    exception.IsOrIsSubType(typeof(InvalidCredentialsException)) || 
						exception.IsOrIsSubType(typeof(MissingCredentialsException));
                    var isForbiddenException = exception.IsOrIsSubType(typeof(ForbiddenException));
                    var isOtherAuthException = exception.IsOrIsSubType(typeof(AuthException));

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
					else if (isUnauthorizedException)
						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					else if (isForbiddenException)
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
					else if (isOtherAuthException)
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
					else
						context.Response.StatusCode = StatusCodes.Status500InternalServerError;

					// Respond
					await context.Response.WriteAsync(JsonSerializer.Serialize(failureResponse));

				}));

            return app;
		}

		private static IApplicationBuilder AddNswagMiddleware(this IApplicationBuilder app, ISettings settings)
		{

			app.UseOpenApi(configure =>
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
					c.DocExpansion = "none";
				});
			return app;
		}
		
		private static void StartContext(this IApplicationBuilder app)
		{
			app.StartCommonAdapters();
			app.StartSecondaryAdapters();
			app.StartPrimaryAdapters();
		}
		
		private static IApplicationBuilder StartCommonAdapters(this IApplicationBuilder app)
		{
			app.StartInterchangeEventAdapter();
			app.StartDomainEventAdapter();
			return app;
		}

		private static IApplicationBuilder StartPrimaryAdapters(this IApplicationBuilder app)
		{
			app.StartListeners();
			return app;
		}

		private static IApplicationBuilder StartSecondaryAdapters(this IApplicationBuilder app)
		{
			return app;
		}

		private static IApplicationBuilder StartListeners(this IApplicationBuilder app)
		{
			foreach (var listener in app.ApplicationServices.GetServices<IEventListener>())
				listener.StartAsync().Wait();
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
		
		private static void StopContext(this IApplicationBuilder app)
		{
			app.StopPrimaryAdapters();
			app.StopSecondaryAdapters();
			app.StopCommonAdapters();
		}
		
		private static IApplicationBuilder StopPrimaryAdapters(this IApplicationBuilder app)
		{
			app.StopListeners();
			return app;
		}

		private static IApplicationBuilder StopSecondaryAdapters(this IApplicationBuilder app)
		{
			return app;
		}
		
		private static IApplicationBuilder StopCommonAdapters(this IApplicationBuilder app)
		{
			app.StopInterchangeEventAdapter();
			app.StopDomainEventAdapter();
			return app;
		}
		
		private static IApplicationBuilder StopListeners(this IApplicationBuilder app)
		{
			foreach (var listener in app.ApplicationServices.GetServices<IEventListener>())
				listener.StopAsync().Wait();
			return app;
		}

		private static IApplicationBuilder StopInterchangeEventAdapter(this IApplicationBuilder app)
		{
			var interchangeEventAdapter =
				app.ApplicationServices.GetService<IInterchangeEventAdapter>();
		
			if (interchangeEventAdapter != null)
				interchangeEventAdapter.StopAsync().Wait();
		
			return app;
		}

		private static IApplicationBuilder StopDomainEventAdapter(this IApplicationBuilder app)
		{
			var domainEventAdapter =
				app.ApplicationServices.GetService<IDomainEventAdapter>();

			if (domainEventAdapter != null)
				domainEventAdapter.StopAsync().Wait();

			return app;
		}
	}
}