using System.Reflection;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using DDD.Application.Settings;
using DDD.DotNet.Extensions;
using Application.Actions;
using Application.Actions.Commands;
using Domain.Model.Forecast;
using Infrastructure.Ports.Adapters.Domain;
using Infrastructure.Ports.Adapters.Interchange.Translation;
using Infrastructure.Ports.Adapters.Repositories.Memory;
using Infrastructure.Ports.Adapters.Repositories.Migration;
using Infrastructure.Ports.Adapters.Repositories.Postgres;
using HttpV1_0_0Translation = Infrastructure.Ports.Adapters.Http.v1_0_0.Translation;

// Create & Start Application
var builder = WebApplication.CreateBuilder(args);
ISettings settings = null;

ConfigureWebHostBuilder(builder);
ReadSettings(builder);

// Adapt Startup.cs (.NET < 6.0)
ConfigureServices(builder);
var app = builder.Build();
Configure(app);

app.Run();

// Helpers

void ReadSettings(WebApplicationBuilder builder)
{
    var options = new Options();
    builder.Configuration.Bind(options);
    var iOptions = MicrosoftOptions.Create(options);
    settings = new Settings(iOptions);
}

void ConfigureWebHostBuilder(WebApplicationBuilder builder)
{
    builder.WebHost.AddEnvFile("ENV_FILE", "CFG_");
    builder.WebHost.AddSettings();
    builder.WebHost.AddLogging();
}

void ConfigureServices(WebApplicationBuilder builder)
{
    var services = builder.Services;

    // DDD.NETCore
    services.AddAccessControl(settings);
    services.AddMonitoring(settings);
    services.AddPersistence(settings);
    services.AddPubSub(settings);

    // App
    AddDomainServices(services);
    AddApplicationService(services);
    AddSecondaryAdapters(services);
    AddPrimaryAdapters(services);
}

void Configure(WebApplication app)
{
    // DDD.NETCore
    app.AddAccessControl(settings);
    app.AddHttpAdapter(settings);
    app.AddTranslation(settings);
    AddControl(app);
}

// DDD.NETCore

void AddControl(WebApplication app)
{
    var lifetime = app.Lifetime;
    lifetime.ApplicationStarted.Register(app.OnAppStarted);
    lifetime.ApplicationStopping.Register(app.OnAppStopping);
    lifetime.ApplicationStopped.Register(app.OnAppStopped);
}

// App

void AddDomainServices(IServiceCollection services)
{
    
}

void AddApplicationService(IServiceCollection services)
{
    AddActions(services);
}

void AddSecondaryAdapters(IServiceCollection services)
{
    services.AddEmailAdapter(settings);
    AddRepositories(services);
}

void AddPrimaryAdapters(IServiceCollection services)
{
    AddHttpAdapters(services);
    AddInterchangeEventAdapters(services);
    AddDomainEventAdapters(services);
}

void AddActions(IServiceCollection services)
{
    services.AddAction<NotifyWeatherPredictedAction, NotifyWeatherPredictedCommand>();
    services.AddAction<PredictWeatherAction, PredictWeatherCommand>();
}

void AddHttpAdapters(IServiceCollection services)
{
    var mvcCoreBuilder = services.AddHttpAdapter(settings);
    AddHttpAdapterV1_0_0(services, mvcCoreBuilder);
}

void AddHttpAdapterV1_0_0(IServiceCollection services, IMvcCoreBuilder mvcCoreBuilder)
{
    mvcCoreBuilder.AddApplicationPart(Assembly.GetAssembly(typeof(Infrastructure.Ports.Adapters.Http.v1_0_0.HttpAdapter)));
    services.AddTransient<HttpV1_0_0Translation.Commands.PredictWeatherCommandTranslator>();
    services.AddTransient<HttpV1_0_0Translation.ForecastIdTranslator>();
    services.AddTransient<HttpV1_0_0Translation.ForecastTranslator>();
}

void AddInterchangeEventAdapters(IServiceCollection services)
{
    services.AddTransient<IIcForecastTranslator, IcForecastTranslator>();
}

void AddDomainEventAdapters(IServiceCollection services)
{
    services.AddListener<WeatherPredictedListener>();
}

void AddRepositories(IServiceCollection services)
{
    if (settings.Persistence.Provider == PersistenceProvider.Memory)
    {
        services.AddRepository<IForecastRepository, MemoryForecastRepository>();
    }
    else if (settings.Persistence.Provider == PersistenceProvider.Postgres)
    {
        services.AddRepository<IForecastRepository, PostgresForecastRepository>();
    }
    services.AddTransient<ForecastMigrator>();
}
