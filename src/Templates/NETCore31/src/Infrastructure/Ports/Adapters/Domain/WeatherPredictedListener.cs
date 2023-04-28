using Application.Actions;
using Application.Actions.Commands;
using OpenDDD.Application;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Logging;
using Domain.Model.Forecast;
using WeatherDomainModelVersion = Domain.Model.DomainModelVersion;

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
			ILogger logger,
			SerializerSettings serializerSettings)
			: base(
				Context.Domain,
				"WeatherPredicted",
				WeatherDomainModelVersion.Latest(),
				action,
				eventAdapter,
				outbox,
				deadLetterQueue,
				logger,
				serializerSettings)
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
					SummaryId = theEvent.SummaryId
				};

			return command;
		}
	}
}
