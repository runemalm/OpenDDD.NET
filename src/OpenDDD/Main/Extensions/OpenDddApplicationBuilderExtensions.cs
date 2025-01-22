using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenDDD.Main.Options;

namespace OpenDDD.Main.Extensions
{
    public static class OpenDddApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOpenDDD(this IApplicationBuilder app)
        {
            // Resolve OpenDddOptions (can be used to configure the pipeline if needed)
            var options = app.ApplicationServices.GetRequiredService<IOptions<OpenDddOptions>>().Value;

            // Placeholder for pipeline configuration (e.g., add middleware based on options)

            return app;
        }
    }
}
