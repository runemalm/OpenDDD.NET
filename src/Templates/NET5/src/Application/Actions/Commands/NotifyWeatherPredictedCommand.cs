using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Application;
using DDD.Application.Error;
using DDD.Domain.Model.Validation;
using Domain.Model.Forecast;
using Domain.Model.Summary;

namespace Application.Actions.Commands
{
    public class NotifyWeatherPredictedCommand : Command
    {
        public ForecastId ForecastId { get; set; }
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public SummaryId SummaryId { get; set; }

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
                .NotNullOrEmpty(command => command.SummaryId.Value)
                .Errors();

            return errors;
        }
    }
}
