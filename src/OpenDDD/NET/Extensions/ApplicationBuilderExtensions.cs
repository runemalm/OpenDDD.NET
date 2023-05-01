using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using OpenDDD.Application.Error;
using OpenDDD.Application.Settings;
using OpenDDD.Domain.Model.Auth.Exceptions;
using OpenDDD.Domain.Model.Error;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Infrastructure.Ports.Repository;
using OpenDDD.Infrastructure.Services.Persistence;
using OpenDDD.Infrastructure.Services.Publisher;
using OpenDDD.NET.Hooks;
using OpenDDD.NET.Middleware;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OpenDDD.NET.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		// Public API

		public static IApplicationBuilder AddAccessControl(this IApplicationBuilder app, ISettings settings)
		{
			app.UseMiddleware<AuthMiddleware>(settings);
			return app;
		}
		
		public static IApplicationBuilder AddControl(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
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

		private static IApplicationBuilder AddHttpAdapterDocs(this IApplicationBuilder app, ISettings settings)
		{
			if (settings.Http.Docs.Enabled)
				app.AddNswagMiddleware(settings);

			return app;
		}

		private static IApplicationBuilder AddHttpAdapterErrorFormatting(this IApplicationBuilder app, ISettings settings)
		{
			app.UseExceptionHandler(
                c => c.Run(async context =>
                {
                    var exception = context.Features
                        .Get<IExceptionHandlerPathFeature>()
                        .Error;

                    var isDomainException = exception.IsOrIsSubType(typeof(DomainException));
                    var isAuthorizeException = exception.IsOrIsSubType(typeof(AuthorizeException));
                    var isUnauthorizedException = isAuthorizeException &&
                                                  ((AuthorizeException)exception).Errors.Any(e =>
	                                                  e.Code == DomainError.Authorize_MissingCredentials_Code || 
	                                                  e.Code == DomainError.Authorize_InvalidCredentials_Code);
                    var isForbiddenException = isAuthorizeException &&
                                               ((AuthorizeException)exception).Errors.Any(e =>
	                                               e.Code == DomainError.Authorize_Forbidden_Code);
                    var isNotFoundException = isDomainException &&
                                              ((DomainException)exception).Errors.Any(e =>
	                                              e.Code == DomainError.Domain_NotFound_Code);
                    var isInvalidCommandException = exception.IsOrIsSubType(typeof(InvalidCommandException));
                    var isInvariantException = isDomainException &&
                                               ((DomainException)exception).Errors.Any(e =>
	                                               e.Code == DomainError.Domain_InvariantViolation_Code);

                    // Prepare response
					if (!context.Response.HasStarted)
						context.Response.Clear();

					context.Response.ContentType = "application/json";

					// Create failure response
					Failure failureResponse;

					if (isDomainException)
					{
						failureResponse = new Failure(
							((DomainException)exception).Errors);
					}
					else
					{
						failureResponse = new Failure(
							ApplicationError.System_UnknownError(exception.Message)); }

					// Set http status code
					if (isNotFoundException)
						context.Response.StatusCode = StatusCodes.Status404NotFound;
					else if (isInvalidCommandException)
						context.Response.StatusCode = StatusCodes.Status400BadRequest;
					else if (isInvariantException)
						context.Response.StatusCode = StatusCodes.Status400BadRequest;
					else if (isUnauthorizedException)
						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					else if (isForbiddenException)
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
					else if (isAuthorizeException)
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
					else if (isDomainException)
						context.Response.StatusCode = StatusCodes.Status400BadRequest;
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

						if (!settings.Http.Docs.Hostname.IsNullOrEmpty())
						{
							var port = 0;
							if (settings.Http.Docs.HttpEnabled)
								port = settings.Http.Docs.HttpPort != 80 ? settings.Http.Docs.HttpPort : 0;
							else if (settings.Http.Docs.HttpsEnabled)
								port = settings.Http.Docs.HttpsPort != 443 ? settings.Http.Docs.HttpsPort : 0;

							document.Host = $"{settings.Http.Docs.Hostname}{(port != 0 ? ":"+port : "")}";
						}
					};
				})
				.UseSwaggerUi3(c =>
				{
					c.DocExpansion = "none"; // 'list', 'full' or 'none'
				});
			return app;
		}
		
		public static IApplicationBuilder StartContext(this IApplicationBuilder app)
		{
			app.StartCommonAdapters();
			app.StartSecondaryAdapters();
			app.OnBeforePrimaryAdaptersStarted();
			app.StartPrimaryAdapters();
			return app;
		}
		
		public static IApplicationBuilder OnBeforePrimaryAdaptersStarted(this IApplicationBuilder app)
		{
			var serviceProvider = app.ApplicationServices;
			var hooks = serviceProvider.GetServices<IOnBeforePrimaryAdaptersStartedHook>();
			foreach (var hook in hooks)
				hook.Execute(app);
			return app;
		}
		
		private static IApplicationBuilder StartCommonAdapters(this IApplicationBuilder app)
		{
			app.StartPersistenceService();
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
			app.StartOutbox();
			app.StartRepositories();
			return app;
		}
		
		private static IApplicationBuilder StartPersistenceService(this IApplicationBuilder app)
		{
			var persistenceService = app.ApplicationServices.GetService<IPersistenceService>();
			persistenceService.Start();
			return app;
		}
		
		private static IApplicationBuilder StartOutbox(this IApplicationBuilder app)
		{
			var outbox = app.ApplicationServices.GetService<IOutbox>();
			outbox.Start(CancellationToken.None);
			app.StartPublisherService();
			return app;
		}
		
		private static IApplicationBuilder StartPublisherService(this IApplicationBuilder app)
		{
			var publisherService = app.ApplicationServices.GetService<IPublisherService>();
			publisherService.Start();
			return app;
		}

		private static IApplicationBuilder StartRepositories(this IApplicationBuilder app)
		{
			foreach (var repository in app.ApplicationServices.GetServices<IStartableRepository>())
			{
				repository.Start(CancellationToken.None);
			}
			return app;
		}

		private static IApplicationBuilder StartListeners(this IApplicationBuilder app)
		{
			foreach (var listener in app.ApplicationServices.GetServices<IEventListener>())
				listener.Start();
			return app;
		}

		private static IApplicationBuilder StartInterchangeEventAdapter(this IApplicationBuilder app)
		{
			var interchangeEventAdapter = app.ApplicationServices.GetService<IInterchangeEventAdapter>();
			if (interchangeEventAdapter != null)
				interchangeEventAdapter.Start();
			return app;
		}

		private static IApplicationBuilder StartDomainEventAdapter(this IApplicationBuilder app)
		{
			var domainEventAdapter = app.ApplicationServices.GetService<IDomainEventAdapter>();
			if (domainEventAdapter != null)
				domainEventAdapter.Start();
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
			app.StopOutbox();
			app.StopRepositories();
			return app;
		}
		
		private static IApplicationBuilder StopCommonAdapters(this IApplicationBuilder app)
		{
			app.StopInterchangeEventAdapter();
			app.StopDomainEventAdapter();
			app.StopPersistenceService();
			return app;
		}
		
		private static IApplicationBuilder StopListeners(this IApplicationBuilder app)
		{
			foreach (var listener in app.ApplicationServices.GetServices<IEventListener>())
				listener.StopAsync().Wait();
			return app;
		}
		
		private static IApplicationBuilder StopPersistenceService(this IApplicationBuilder app)
		{
			var persistenceService = app.ApplicationServices.GetService<IPersistenceService>();
			persistenceService.Stop();
			return app;
		}
		
		private static IApplicationBuilder StopOutbox(this IApplicationBuilder app)
		{
			var outbox = app.ApplicationServices.GetService<IOutbox>();
			outbox.Stop(CancellationToken.None);
			app.StopPublisherService();
			return app;
		}
		
		private static IApplicationBuilder StopPublisherService(this IApplicationBuilder app)
		{
			var publisherService = app.ApplicationServices.GetService<IPublisherService>();
			publisherService.Stop();
			return app;
		}

		private static IApplicationBuilder StopRepositories(this IApplicationBuilder app)
		{
			foreach (var repository in app.ApplicationServices.GetServices<IStartableRepository>())
				repository.Stop(CancellationToken.None);
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
