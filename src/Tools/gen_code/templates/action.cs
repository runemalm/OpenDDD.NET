using System.Threading;
using System.Threading.Tasks;
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
    public class {{class_name}} : DDD.Application.Action<{{command_name}}, {{return_class_name}}>
    {
        private readonly ISettings _settings;
        private readonly ILogger _logger;

        public {{class_name}}(
            IAuthDomainService authDomainService,
            ISettings settings,
            ILogger logger,
            IDomainPublisher domainPublisher,
            IInterchangePublisher interchangePublisher,
            IOutbox outbox,
            IPersistenceService persistenceService) 
            : base(authDomainService, domainPublisher, interchangePublisher, outbox, persistenceService)
        {
            _settings = settings;
            _logger = logger;
        }

        public override async Task<{{return_class_name}}> ExecuteAsync(
            {{command_name}} command,
            ActionId actionId,
            CancellationToken ct)
        {
            throw new System.NotImplementedException("Auto-generated action has not been implemented.");

            // // Run
            // // ...
            // var xxx = {{return_class_name}}.Create(_settings);
            //
            // // Persist
            // await ...
            //
            // // Return
            // return Task.FromResult(xxx);
        }
    }
}
