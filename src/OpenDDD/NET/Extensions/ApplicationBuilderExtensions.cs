using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application;
using OpenDDD.Application.Error;
using OpenDDD.Domain.Model.Error;
using OpenDDD.Main;
using OpenDDD.NET.Services.DatabaseConnection;
using ApplicationException = OpenDDD.Application.Error.ApplicationException;

namespace OpenDDD.NET.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		// Public API
		
		public static IApplicationBuilder UseBaseSwagger(this IApplicationBuilder app, bool http, bool https, string hostname)
		{

			app.UseOpenApi(configure =>
				{
					configure.PostProcess = (document, request) =>
					{
						
					};
				})
				.UseSwaggerUi3(c =>
				{
					c.DocExpansion = "none"; // 'list', 'full' or 'none'
				});
			return app;
		}

		public static IApplicationBuilder UseBaseExceptionHandler(this IApplicationBuilder app)
		{
			app.UseExceptionHandler(
                c => c.Run(async context =>
                {
                    var exception = context.Features
                        .Get<IExceptionHandlerPathFeature>()
                        .Error;

                    var isApplicationException = exception.IsOrIsSubType(typeof(ApplicationException));
                    var isDomainException = exception.IsOrIsSubType(typeof(DomainException));
                    var isAuthorizeException = exception.IsOrIsSubType(typeof(AuthorizationException));
                    var isUnauthorizedException = isAuthorizeException &&
                                                  ((AuthorizationException)exception).Errors.Any(e =>
	                                                  e.Code == DomainError.Authorization_MissingCredentials_Code || 
	                                                  e.Code == DomainError.Authorization_InvalidCredentials_Code);
                    var isForbiddenException = isAuthorizeException &&
                                               ((AuthorizationException)exception).Errors.Any(e =>
	                                               e.Code == DomainError.Authorization_Forbidden_Code);
                    var isNotFoundException = isDomainException &&
                                              ((DomainException)exception).Errors.Any(e =>
	                                              e.Code == DomainError.Domain_NotFound_Code);
                    var isOldInvalidCommandException = exception.IsOrIsSubType(typeof(InvalidCommandException));
                    var isInvalidCommandException = isOldInvalidCommandException || (
														isApplicationException &&
														((ApplicationException)exception).Errors.Any(e =>
															e.Code == ApplicationError.Application_InvalidCommand_Code));
                    var isInvariantException = isDomainException &&
                                               ((DomainException)exception).Errors.Any(e =>
	                                               e.Code == DomainError.Domain_InvariantViolation_Code);

                    // Prepare response
					if (!context.Response.HasStarted)
						context.Response.Clear();

					context.Response.ContentType = "application/json";

					// Create failure response
					Failure failureResponse;

					if (isApplicationException)
					{
						failureResponse = new Failure(
							((ApplicationException)exception).Errors);
					}
					else if (isDomainException)
					{
						failureResponse = new Failure(
							((DomainException)exception).Errors);
					}
					else
					{
						failureResponse = new Failure(
							ApplicationError.System_InternalError(exception.Message)); }

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
		
		public static IApplicationBuilder RunEnsureDataTasks(this IApplicationBuilder app)
		{
			// Create a scope, since some dependencies are registered as scoped
			using (var scope = app.ApplicationServices.CreateScope())
			{
				// Start action database connection
				var actionDatabaseConnection = scope.ServiceProvider.GetService<IActionDatabaseConnection>();
				if (actionDatabaseConnection == null)
					throw new Exception(
						"Can't run ensure data tasks, IApplicationDatabaseConnection hasn't been registered.");
				actionDatabaseConnection.Start(CancellationToken.None);
				
				// Run the ensure data tasks
				foreach (var task in scope.ServiceProvider.GetServices<IEnsureDataTask>())
					task.Execute(ActionId.BootId(), CancellationToken.None);
				
				// Stop action database connection
				actionDatabaseConnection.Stop(CancellationToken.None);
			}
			
			return app;
		}
		
		public static IApplicationBuilder UseActionMiddleware(this IApplicationBuilder app)
		{
			app.UseMiddleware<ActionMiddleware>();
			return app;
		}
		
		// public static IApplicationBuilder StartActionDatabaseConnection(this IApplicationBuilder app)
		// {
		// 	var actionDatabaseConnection = app.ApplicationServices.GetService<IActionDatabaseConnection>();
		// 	if (actionDatabaseConnection == null)
		// 		throw new Exception("Can't start action database connection, it seems it hasn't been registered.");
		// 	actionDatabaseConnection.Start(CancellationToken.None);
		// 	return app;
		// }

		// public static IApplicationBuilder StartContext(this IApplicationBuilder app)
		// {
		// 	app.StartCommonAdapters();
		// 	app.StartSecondaryAdapters();
		// 	app.StartPrimaryAdapters();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StartCommonAdapters(this IApplicationBuilder app)
		// {
		// 	app.StartPersistenceService();
		// 	app.StartInterchangeEventAdapter();
		// 	app.StartDomainEventAdapter();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StartPrimaryAdapters(this IApplicationBuilder app)
		// {
		// 	app.StartListeners();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StartSecondaryAdapters(this IApplicationBuilder app)
		// {
		// 	app.StartOutbox();
		// 	app.StartRepositories();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StartPersistenceService(this IApplicationBuilder app)
		// {
		// 	var persistenceService = app.ApplicationServices.GetService<IPersistenceService>();
		// 	persistenceService.Start();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StartOutbox(this IApplicationBuilder app)
		// {
		// 	var outbox = app.ApplicationServices.GetService<IOutbox>();
		// 	outbox.Start(CancellationToken.None);
		// 	app.StartPublisherService();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StartPublisherService(this IApplicationBuilder app)
		// {
		// 	var publisherService = app.ApplicationServices.GetService<IPublisherService>();
		// 	publisherService.Start();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StartRepositories(this IApplicationBuilder app)
		// {
		// 	foreach (var repository in app.ApplicationServices.GetServices<IStartableRepository>())
		// 	{
		// 		repository.Start(CancellationToken.None);
		// 	}
		// 	return app;
		// }
		
		public static IApplicationBuilder StartListeners(this IApplicationBuilder app)
		{
			throw new NotImplementedException();
			// foreach (var listener in app.ApplicationServices.GetServices<IListener>())
			// 	listener.Start();
			// return app;
		}
		
		public static IApplicationBuilder StopListeners(this IApplicationBuilder app)
		{
			throw new NotImplementedException();
			// foreach (var listener in app.ApplicationServices.GetServices<IListener>())
			// 	listener.Stop();
			// return app;
		}
		
		// private static IApplicationBuilder StartInterchangeEventAdapter(this IApplicationBuilder app)
		// {
		// 	var interchangeEventAdapter = app.ApplicationServices.GetService<IInterchangeEventAdapter>();
		// 	if (interchangeEventAdapter != null)
		// 		interchangeEventAdapter.Start();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StartDomainEventAdapter(this IApplicationBuilder app)
		// {
		// 	var domainEventAdapter = app.ApplicationServices.GetService<IDomainEventAdapter>();
		// 	if (domainEventAdapter != null)
		// 		domainEventAdapter.Start();
		// 	return app;
		// }
		//
		// private static void StopContext(this IApplicationBuilder app)
		// {
		// 	app.StopPrimaryAdapters();
		// 	app.StopSecondaryAdapters();
		// 	app.StopCommonAdapters();
		// }
		//
		// private static IApplicationBuilder StopPrimaryAdapters(this IApplicationBuilder app)
		// {
		// 	app.StopListeners();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopSecondaryAdapters(this IApplicationBuilder app)
		// {
		// 	app.StopOutbox();
		// 	app.StopRepositories();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopCommonAdapters(this IApplicationBuilder app)
		// {
		// 	app.StopInterchangeEventAdapter();
		// 	app.StopDomainEventAdapter();
		// 	app.StopPersistenceService();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopListeners(this IApplicationBuilder app)
		// {
		// 	foreach (var listener in app.ApplicationServices.GetServices<IEventListener>())
		// 		listener.StopAsync().Wait();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopPersistenceService(this IApplicationBuilder app)
		// {
		// 	var persistenceService = app.ApplicationServices.GetService<IPersistenceService>();
		// 	persistenceService.Stop();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopOutbox(this IApplicationBuilder app)
		// {
		// 	var outbox = app.ApplicationServices.GetService<IOutbox>();
		// 	outbox.Stop(CancellationToken.None);
		// 	app.StopPublisherService();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopPublisherService(this IApplicationBuilder app)
		// {
		// 	var publisherService = app.ApplicationServices.GetService<IPublisherService>();
		// 	publisherService.Stop();
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopRepositories(this IApplicationBuilder app)
		// {
		// 	foreach (var repository in app.ApplicationServices.GetServices<IStartableRepository>())
		// 		repository.Stop(CancellationToken.None);
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopInterchangeEventAdapter(this IApplicationBuilder app)
		// {
		// 	var interchangeEventAdapter =
		// 		app.ApplicationServices.GetService<IInterchangeEventAdapter>();
		//
		// 	if (interchangeEventAdapter != null)
		// 		interchangeEventAdapter.StopAsync().Wait();
		//
		// 	return app;
		// }
		//
		// private static IApplicationBuilder StopDomainEventAdapter(this IApplicationBuilder app)
		// {
		// 	var domainEventAdapter =
		// 		app.ApplicationServices.GetService<IDomainEventAdapter>();
		//
		// 	if (domainEventAdapter != null)
		// 		domainEventAdapter.StopAsync().Wait();
		//
		// 	return app;
		// }
	}
}
