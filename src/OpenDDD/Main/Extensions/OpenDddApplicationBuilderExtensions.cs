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
            var options = app.ApplicationServices.GetRequiredService<IOptions<OpenDddOptions>>().Value;

            app.UseMiddleware<ActionMiddleware>();

            return app;
        }
    }
}
