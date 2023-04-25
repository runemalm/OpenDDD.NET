using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using DDD.NETCore.Extensions;
using Main.Extensions;

namespace Main
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateWebHostBuilder(args).Build().Run();
	    
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseStartup<Startup>()
                .AddEnvFile("ENV_FILE", "CFG_")
                .AddSettings()
                .AddCustomSettings()
                .AddLogging();
    }
}
