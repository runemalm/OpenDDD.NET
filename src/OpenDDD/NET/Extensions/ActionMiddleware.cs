using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenDDD.NET.Services.DatabaseConnection;

namespace OpenDDD.NET.Extensions
{
	public class ActionMiddleware
	{
		private readonly RequestDelegate _next;

		public ActionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
		{
			// Get the connection
			var actionDatabaseConnection = context.RequestServices.GetRequiredService<IActionDatabaseConnection>();
			
			if (actionDatabaseConnection == null)
				throw new Exception("Can't start action database connection, it seems it hasn't been registered.");
			
			// Start the connection before the action execution
			actionDatabaseConnection.Start(CancellationToken.None);

			await _next(context);
			
			// Stop the connection after the action execution
			actionDatabaseConnection.Stop(CancellationToken.None);
		}
	}
}
