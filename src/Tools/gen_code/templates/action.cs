using System.Threading;
using System.Threading.Tasks;
using DDD.Logging;
using DDD.Application.Settings;
using DDD.Application;
using DDD.Domain.Services.Auth;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Infrastructure.Services.Persistence;
using Application.Actions.Commands;
using Domain.Model.Realm;

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

            // // Authorize
            // await _authDomainService.AuthorizeRolesAsync(new[]
            //     {
            //         new[] { "web.superuser" },
            //     }, 
            //     ct);
            //
            // 
            // // Run
            // // ...
            // var xxx = await {{return_class_name}}.CreateAsync({{return_class_name}}Id, command., actionId);
            //
            // // Persist
            // await ...
            //
            // // Return
            // return Task.FromResult(xxx);
        }
    }
}
