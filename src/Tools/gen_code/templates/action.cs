using System.Threading;
using System.Threading.Tasks;
using DDD.Logging;
using DDD.Application;
using DDD.Domain.Auth;
using Application.Actions.Commands;
using DDD.Application.Settings;

namespace Application.Actions
{
    public class {{class_name}} : Action<{{command_name}}, {{return_class_name}}>
    {
        private readonly ISettings _settings;
        private readonly ILogger _logger;

        public {{class_name}}(
            IAuthDomainService authDomainService,
            ISettings settings,
            ILogger logger) : base(authDomainService)
        {
            _settings = settings;
            _logger = logger;
        }

        [Transactional]
        public override Task<{{return_class_name}}> ExecuteAsync(
            {{command_name}} command,
            CancellationToken ct)
        {
            throw new System.NotImplementedException("Auto-generated action has not been implemented.");

            // // Validate command
            // Validate(command);
            //
            // // Run
            // // ...
            // var xxx = {{return_class_name}}.Create(_settings);
            //
            // // Return
            // return Task.FromResult(xxx);
        }
    }
}
