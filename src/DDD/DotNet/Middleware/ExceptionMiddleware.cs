using System.Threading.Tasks;
using DDD.Logging;
using DDD.Application.Settings;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DDD.DotNet.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		public ExceptionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			//credentials.JwtToken = null;

			//if (_settings.Auth.Enabled)
			//{
			//	if (_settings.Auth.Provider.ToLower() == "jwt")
			//	{
			//		var jwtToken = GetJwtToken(context);
			//		credentials.JwtToken = jwtToken;
			//	}
			//}

			//var response = new { error = "TODO: .... exception.Message" };
			//await WriteResponseAsync(response, StatusCodes.Status408RequestTimeout);

			if (!context.Response.HasStarted)
				context.Response.Clear();
			context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
			context.Response.ContentType = "application/json";

			await context.Response.WriteAsync("TODO ... _serializer.Serialize(response)");

			await _next(context);
		}



		//private readonly RequestDelegate _requestDelegate;
		//private readonly ILogger _logger;
		//private readonly JsonSerializer _serializer = new JsonSerializer();

		//public ExceptionMiddleware(RequestDelegate requestDelegate, ILogger logger)
		//{
		//	_requestDelegate = requestDelegate;
		//	_logger = logger;
		//}

		//public async Task Invoke(HttpContext context)
		//{
		//	try
		//	{
		//		await _requestDelegate(context);
		//	}
		//	catch (Exception ex)
		//	{
		//		try
		//		{
		//			//var logHandler = new LogHandler(_logger);
		//			//var exceptionInfoFactory = new ExceptionInfoFactory();
		//			//var exceptionInfo = exceptionInfoFactory.Create(ex, LanguageUtil.GetFirstSupportedLanguage(context.Request.GetTypedHeaders().AcceptLanguage, new[] { Language.En, Language.Sv }));
		//			//logHandler.Log(exceptionInfo);
		//			//var response = _exceptionResultRenderer.Render(exceptionInfo, _options.Value.DebugErrorResponse);
					//var response = new { error = "TODO: .... exception.Message" };
					//await WriteResponseAsync(response, StatusCodes.Status408RequestTimeout);
		//		}
		//		catch (Exception criticalEx)
		//		{
		//			var message = "Internal error in ExceptionMiddleware";
		//			//_logger.Log(message, LoggingLevel.Critical, criticalEx);
		//			await WriteResponseAsync(new { Message = message + ". See log for details." }, 500);
		//		}
		//	}

		//	async Task WriteResponseAsync<TResponse>(TResponse response, int statusCode)
		//	{
		//		if (!context.Response.HasStarted)
		//			context.Response.Clear();
		//		context.Response.StatusCode = statusCode;
		//		context.Response.ContentType = "application/json";

		//		await context.Response.WriteAsync("TODO ... _serializer.Serialize(response)");
		//	}
		//}
	}
}
