using Microsoft.AspNetCore.Builder;
using OpenDDD.Main.Options;

namespace OpenDDD.Main.Extensions
{
    public static class OpenDddApplicationBuilderExtensions
    {
        public static void UseOpenDDD(this IApplicationBuilder app)
        {
            // Placeholder for now until we have something to add to the pipeline..
        }

        public static IApplicationBuilder UseOpenDDD(this IApplicationBuilder app, Action<OpenDddPipelineOptions>? configureOptions)
        {
            var options = new OpenDddPipelineOptions();
            configureOptions?.Invoke(options);
            return app;
        }
    }
}
