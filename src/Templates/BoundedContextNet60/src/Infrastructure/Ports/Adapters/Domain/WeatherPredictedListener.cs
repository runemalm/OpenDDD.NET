using Application.Actions;
using Application.Actions.Commands;
using DDD.Infrastructure.Ports;
using DDD.Application;
using DDD.Logging;
using Domain.Model.Forecast;
using StudyDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Infrastructure.Ports.Adapters.Domain
{
	public class WeatherPredictedListener
		: EventListener<WeatherPredicted, NotifyWeatherPredictedAction, NotifyWeatherPredictedCommand>
	{
		public WeatherPredictedListener(
			NotifyWeatherPredictedAction action,
			IDomainEventAdapter eventAdapter,
			IOutbox outbox,
			IDeadLetterQueue deadLetterQueue,
			ILogger logger)
			: base(
				Context.Domain,
				"WeatherPredicted",
				StudyDomainModelVersion.Latest(),
				action,
				eventAdapter,
				outbox,
				deadLetterQueue,
				logger)
		{
			
		}
		
		public override NotifyWeatherPredictedCommand CreateCommand(WeatherPredicted theEvent)
		{
			var command =
				new NotifyWeatherPredictedCommand()
				{
					ForecastId = theEvent.ForecastId,
					Date = theEvent.Date,
					TemperatureC = theEvent.TemperatureC,
					Summary = theEvent.Summary
				};

			return command;
		}
	}
}
