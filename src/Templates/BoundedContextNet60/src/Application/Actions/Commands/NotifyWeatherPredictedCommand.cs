using DDD.Application;
using DDD.Domain.Exceptions;
using DDD.Domain.Validation;
using Domain.Model.Forecast;

namespace Application.Actions.Commands
{
    public class NotifyWeatherPredictedCommand : Command
    {
        public ForecastId ForecastId { get; set; }
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }

        public override void Validate()
        {
            var errors = GetErrors();

            if (errors.Any())
                throw new InvalidCommandException(this, errors);
        }

        public override IEnumerable<ValidationError> GetErrors()
        {
            var errors = new Validator<NotifyWeatherPredictedCommand>(this)
                .NotNullOrEmpty(command => command.ForecastId.Value)
                .NotNullOrEmpty(command => command.Summary)
                .Errors();

            return errors;
        }
    }
}
