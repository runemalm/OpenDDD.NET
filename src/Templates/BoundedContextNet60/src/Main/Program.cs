using DDD.Application.Settings;
using DDD.NETCore.Extensions;
using Main;
using Main.Extensions;
using Application.Settings;
using HttpCommonTranslation = Infrastructure.Ports.Adapters.Http.Common.Translation;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using ILogger = DDD.Logging.ILogger;

// Create application builder
var builder = WebApplication.CreateBuilder(args);

// Configure web host
builder.WebHost.AddEnvFile("ENV_FILE", "CFG_");
builder.WebHost.AddSettings();
builder.WebHost.AddCustomSettings();
builder.WebHost.AddLogging();

// Build the service provider
// var sp = builder.Services.BuildServiceProvider();

// // Resolve dependencies
// var settings = sp.GetService<ISettings>();
// var customSettings = sp.GetService<ICustomSettings>();
// var logger = sp.GetService<ILogger>();
//
// // Create startup
// var startup = new Startup(settings, customSettings, logger);

var startup = new Startup(builder.Configuration);

// Configure application services
startup.ConfigureServices(builder.Services);

// Build application
var app = builder.Build();

// Configure application middleware
startup.Configure(app, app.Environment, app.Lifetime);

// Run application
app.Run();
