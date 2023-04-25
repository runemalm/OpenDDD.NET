using DDD.Logging;
using DDD.Application;
using DDD.Application.Settings;
using Application.Actions.Commands;
using DDD.Domain.Model.Error;
using DDD.Domain.Services.Auth;
using DDD.Infrastructure.Ports.Email;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Infrastructure.Services.Persistence;

namespace Application.Actions
{
    public class NotifyWeatherPredictedAction : Action<NotifyWeatherPredictedCommand, object>
    {
        public NotifyWeatherPredictedAction(ActionDependencies deps) : base(deps)
        {
            
        }

        public override async Task<object> ExecuteAsync(
            NotifyWeatherPredictedCommand command,
            ActionId actionId,
            CancellationToken ct)
        {
            // TODO: Add authorization

            // Run
            var summary = 
                await _summaryRepository.GetAsync(command.SummaryId, actionId, ct);
            
            if (summary == null)
                throw DomainException.InvariantViolation(
                    $"Can't send email, summary with ID '{command.SummaryId}' don't exist.");
            
            await _emailAdapter.SendAsync(
                fromEmail: "no-reply@weatherforecast.com",
                fromName: "My Domain",
                toEmail: "bob@example.com",
                toName: "Bob Andersson",
                subject: "Weather was predicted",
                message: $"Hi, the weather tomorrow will be '{summary.Value}' ({command.TemperatureC}°C).",
                isHtml: true,
                ct: ct);
            
            // Return
            return Task.FromResult(new object());
        }
    }
}
