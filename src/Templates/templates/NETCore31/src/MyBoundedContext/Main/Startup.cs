using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Services.Serialization;
using OpenDDD.NET.Extensions;
using MyBoundedContext.Domain.Model.Property;
using MyBoundedContext.Domain.Model.Site;
using MyBoundedContext.Infrastructure.Ports.Adapters.Http.Dto;
using MyBoundedContext.Infrastructure.Ports.Adapters.Repository;
using MyBoundedContext.Infrastructure.Ports.Adapters.Site.Idealista;
using MyBoundedContext.Infrastructure.Ports.Adapters.Site.ThailandProperty;
using MyBoundedContext.Vertical;

namespace MyBoundedContext.Main
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure logging
            services.AddLogging(config =>
            {
                config.AddDebug();
                config.AddConsole();
            });
            
            // HTTP
            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpClient();
            services.AddSwaggerDocuments(new []{ 1 }, new Serializer(new SerializerSettings()));
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy => { policy
                        .WithOrigins("http://localhost:5000", "https://localhost:5001")
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST", "PUT", "DELETE");
                    });
            });

            // Add ensure data tasks
            services.AddEnsureDataTask<EnsureSitesTask>();

            // Date/Time
            services.AddDateTimeProvider();
            
            // Translation & Serialization
            services.AddHttpTranslation<HttpTranslation>();
            services.AddSerializer<SerializerSettings>();
            
            // Event processor  
            services.AddEventProcessorHostedService(Configuration);
            // services.AddEventProcessorDatabaseConnection(Configuration);  
            // services.AddEventProcessorMessageBrokerConnection(Configuration);  
            // services.AddEventProcessorOutbox();  
			  
            // Database
            services.MaybeAddDatabase(Configuration);

            // Action services
            services.AddActionDatabaseConnection(Configuration);  
            // services.AddActionMessageBrokerConnection(Configuration);  
            services.AddActionOutbox();

            // Actions
            services.AddAction<CrawlSearchPage.Action, CrawlSearchPage.Command>();
            services.AddAction<GetProperties.Action, GetProperties.Command>();
            
            // Publishers
            services.AddDomainPublisher();
            // services.AddIntegrationPublisher(Configuration);
            
            // Listeners
            // services.AddDomainEventListener(SearchPageCrawledListener);
            // services.AddIntegrationEventListener(IcAccountCreatedListener);
            
            // Repositories
            services.AddRepository<IPropertyRepository, PropertyRepository>();
            services.AddRepository<ISiteRepository, SiteRepository>();
            
            // Site adapters
            services.AddTransient<IIdealistaPort, IdealistaAdapter>();
            services.AddTransient<IThailandPropertyPort, ThailandPropertyAdapter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseBaseExceptionHandler();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Add the action middleware
            app.UseActionMiddleware();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseBaseSwagger(true, true, "localhost:5001");

            // Hook into application shutdown event
            appLifetime.ApplicationStarted.Register(() => OnApplicationStarted(app));
            appLifetime.ApplicationStopping.Register(() => OnApplicationStopping(app));
        }

        private void OnApplicationStarted(IApplicationBuilder app)
        {
            // Startup tasks
            app.RunEnsureDataTasks();
            
            // Start dependencies
            // app.StartSecondaryAdapters();
            // app.StartPrimaryAdapters();
        }

        private void OnApplicationStopping(IApplicationBuilder app)
        {
            // Stop dependencies
            // app.StopPrimaryAdapters();
            // app.StopSecondaryAdapters();
        }
    }
}
