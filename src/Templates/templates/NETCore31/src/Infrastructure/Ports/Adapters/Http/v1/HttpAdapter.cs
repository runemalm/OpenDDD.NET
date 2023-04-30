using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenDDD.Infrastructure.Ports.Adapters.Http.Common;
using OpenDDD.Infrastructure.Ports.Adapters.Http.NETCore;
using Application.Actions;
using Infrastructure.Ports.Adapters.Http.Common;
using Infrastructure.Ports.Adapters.Http.Common.Translation;
using Infrastructure.Ports.Adapters.Http.Common.Translation.Commands;
using Infrastructure.Ports.Adapters.Http.v1.Model;
using Infrastructure.Ports.Adapters.Http.v1.Model.Commands;

namespace Infrastructure.Ports.Adapters.Http.v1
{
	[Route("v1")]
	[Version("v1.0.0")]
	public class HttpAdapter : NETCoreHttpAdapter
	{
		private readonly GetAverageTemperatureAction _getAverageTemperatureAction;
		private readonly GetAverageTemperatureCommandTranslator _getAverageTemperatureCommandTranslator;
		private readonly PredictWeatherAction _predictWeatherAction;
		private readonly PredictWeatherCommandTranslator _predictWeatherCommandTranslator;
		private readonly ForecastTranslator _forecastTranslator;
		
		public HttpAdapter(
			GetAverageTemperatureAction getAverageTemperatureAction,
			GetAverageTemperatureCommandTranslator getAverageTemperatureCommandTranslator,
			PredictWeatherAction predictWeatherAction,
			PredictWeatherCommandTranslator predictWeatherCommandTranslator,
			ForecastTranslator forecastTranslator)
		{
			_getAverageTemperatureAction = getAverageTemperatureAction;
			_getAverageTemperatureCommandTranslator = getAverageTemperatureCommandTranslator;
			_predictWeatherAction = predictWeatherAction;
			_predictWeatherCommandTranslator = predictWeatherCommandTranslator;
			_forecastTranslator = forecastTranslator;
		}
		
		/// <remarks>
		/// Get the average temperatures of all predictions.
		/// </remarks>
		/// <returns>
		/// Returns the average temperature.
		/// </returns>
		[Public]
		[Protected]
		[Section("Forecasting")]
		[HttpGet("get-average-temperature")]
		[Returns(typeof(int))]
		public async Task<IActionResult> GetAverageTemperature(
			[FromQuery] GetAverageTemperatureCommandV1 commandV1, CancellationToken ct)
		{
			var command = _getAverageTemperatureCommandTranslator.FromV1(commandV1);
			var averageTemp = await _getAverageTemperatureAction.ExecuteAsync(command, ct);
			return Ok(averageTemp);
		}
		
		/// <remarks>
		/// Make a weather prediction.
		/// </remarks>
		/// <returns>
		/// Returns the prediction.
		/// </returns>
		[Public]
		[Protected]
		[Section("Forecasting")]
		[HttpGet("predict-weather")]
		[Returns(typeof(ForecastV1))]
		public async Task<IActionResult> PredictWeather(
			[FromQuery] PredictWeatherCommandV1 commandV1, CancellationToken ct)
		{
			var command = _predictWeatherCommandTranslator.FromV1(commandV1);
			var forecast = await _predictWeatherAction.ExecuteAsync(command, ct);
			return Ok(_forecastTranslator.ToV1(forecast));
		}
	}
}
