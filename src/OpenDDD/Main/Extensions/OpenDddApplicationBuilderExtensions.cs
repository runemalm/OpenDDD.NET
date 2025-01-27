using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenDDD.Main.Middleware;
using OpenDDD.Main.Options;

namespace OpenDDD.Main.Extensions
{
    public static class OpenDddApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOpenDDD(this IApplicationBuilder app)
        {
            // Resolve OpenDddOptions (can be used to configure the pipeline if needed)
            var options = app.ApplicationServices.GetRequiredService<IOptions<OpenDddOptions>>().Value;

            // Middleware to commit or rollback Unit of Work
            app.UseMiddleware<UnitOfWorkMiddleware>();
            app.UseMiddleware<TransactionalOutboxMiddleware>();

            return app;
        }
    }
}
