using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenDDD.API.Middleware;
using OpenDDD.API.Options;

namespace OpenDDD.API.Extensions
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
