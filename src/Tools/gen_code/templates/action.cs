using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using Application.Actions.Commands;

namespace Application.Actions
{
    public class {{class_name}} : Action<{{command_name}}, bool>
    {
        public {{class_name}}(ActionDependencies deps) : base(deps)
        {
            
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
