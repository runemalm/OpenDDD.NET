using Application.Actions;
using Microsoft.AspNetCore.Mvc;
using DDD.Infrastructure.Ports.Adapters.DotNet;
using DDD.Infrastructure.Ports.Adapters.Http;
using Infrastructure.Ports.Adapters.Http.v1_0_0.Model;
using Infrastructure.Ports.Adapters.Http.v1_0_0.Model.Commands;
using Infrastructure.Ports.Adapters.Http.v1_0_0.Translation;
using Infrastructure.Ports.Adapters.Http.v1_0_0.Translation.Commands;

namespace Infrastructure.Ports.Adapters.Http.v1_0_0;

[Route("v1.0.0")]
public class HttpAdapter : DotNetHttpAdapter
{
	private readonly PredictWeatherAction _predictWeatherAction;
	private readonly PredictWeatherCommandTranslator _predictWeatherCommandTranslator;
	private readonly ForecastTranslator _forecastTranslator;
	
	public HttpAdapter(
		PredictWeatherAction predictWeatherAction,
		PredictWeatherCommandTranslator predictWeatherCommandTranslator,
		ForecastTranslator forecastTranslator)
	{
		_predictWeatherAction = predictWeatherAction;
		_predictWeatherCommandTranslator = predictWeatherCommandTranslator;
		_forecastTranslator = forecastTranslator;
	}
	
	/// <remarks>
	/// Make a weather prediction.
	/// </remarks>
	/// <returns>
	/// Returns the prediction.
	/// </returns>
	[Public]
	[Protected]
	[HttpGet("predict-weather")]
	[Returns(typeof(Forecast_v1_0_0))]
	public async Task<IActionResult> PredictWeather(
		[FromQuery] PredictWeatherCommand_v1_0_0 command_v1_0_0, CancellationToken ct)
	{
		var command = _predictWeatherCommandTranslator.From_v1_0_0(command_v1_0_0);
		var forecast = await _predictWeatherAction.ExecuteAsync(command, ct);
		return Ok(_forecastTranslator.To_v1_0_0(forecast));
	}
}
