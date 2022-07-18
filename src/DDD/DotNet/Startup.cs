using System.Text.Json.Serialization;
using DDD.Application.Settings;
using DDD.Domain;
using DDD.DotNet.Extensions;
using DDD.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DDD.DotNet
{
    public abstract class Startup
    {
        protected ISettings _settings;
        protected ILogger _logger;
        protected IMvcCoreBuilder _mvcCoreBuilder;
        
        public Startup(
            ISettings settings,
            ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            _mvcCoreBuilder = AddDotNetCoreWebApi(services);
            
            services.AddDdd(_settings, DomainModelVersion.Latest());
            
            AddPrimaryAdapters(services);
            AddSecondaryAdapters(services);
            AddApplicationService(services);
            AddDomainServices(services);
            AddRepositories(services);
        }
        
        protected IMvcCoreBuilder AddDotNetCoreWebApi(IServiceCollection services)
        {
            var builder =
                services
                    .AddMvcCore(
                    config =>
                        {
                            
                        })
                    .AddJsonOptions(opts =>
                        {
                            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        });
            return builder;
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationLifetime applicationLifetime)
        {
            // DDD
            app.AddDdd(_settings, applicationLifetime);
            
            // .Net Core Web API
            AddDotNetCoreWebApi(app);
        }
        
        protected void AddDotNetCoreWebApi(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        protected abstract void AddApplicationService(IServiceCollection services);
        protected abstract void AddDomainServices(IServiceCollection services);
        protected abstract void AddRepositories(IServiceCollection services);
        protected abstract void AddPrimaryAdapters(IServiceCollection services);
        protected abstract void AddSecondaryAdapters(IServiceCollection services);
    }
}
