using DDD.Logging;
using DDD.Application;
using DDD.Domain.Auth;
using DDD.Application.Settings;
using DDD.Domain;
using DDD.Infrastructure.Persistence;
using DDD.Infrastructure.Ports;
using Application.Actions.Commands;

namespace Application.Actions
{
    public class NotifyWeatherPredictedAction : DDD.Application.Action<NotifyWeatherPredictedCommand, object>
    {
        private readonly ISettings _settings;
        private readonly ILogger _logger;
        private readonly IEmailPort _emailAdapter;

        public NotifyWeatherPredictedAction(
            IAuthDomainService authDomainService,
            ISettings settings,
            ILogger logger,
            IEmailPort emailAdapter,
            IDomainPublisher domainPublisher,
            IInterchangePublisher interchangePublisher,
            IOutbox outbox,
            IPersistenceService persistenceService) 
            : base(authDomainService, domainPublisher, interchangePublisher, outbox, persistenceService)
        {
            _settings = settings;
            _logger = logger;
            _emailAdapter = emailAdapter;
        }

        public override async Task<object> ExecuteAsync(
            NotifyWeatherPredictedCommand command,
            ActionId actionId,
            CancellationToken ct)
        {
            // Run
            await _emailAdapter.SendAsync(
                "auto@boundedcontext.com", 
                "My Domain", 
                "bob@example.com", 
                "Bob Andersson", 
                "Weather was predicted", 
                $"Hi, the weather tomorrow will be '{command.Summary}' ({command.TemperatureC}°C).",
                ct);
            
            // Return
            return Task.FromResult(new object());
        }
    }
}
