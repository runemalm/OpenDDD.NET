using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DDD.NETCore.Hooks;
using DDD.NETCore.Extensions;
using DDD.Application.Settings;
using Main;
using Main.Extensions;
using Application.Actions;
using Application.Actions.Commands;
using Application.Settings;
using Domain.Model.Forecast;
using Domain.Model.Notification;
using Domain.Model.Summary;
using Main.NETCore.Hooks;
using DddActionUnitTests = DDD.Tests.ActionUnitTests;
using ILogger = DDD.Logging.ILogger;

namespace Tests
{
    public class ActionUnitTests : DddActionUnitTests
    {
        protected Forecast Forecast => Forecasts.First();
        protected List<Forecast> Forecasts = new();

        // Setup

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseKestrel();
            builder.WebHost.AddEnvFile($"ENV_FILE_{ActionName}", $"CFG_{ActionName}_", "", false);
            builder.WebHost.AddSettings();
            builder.WebHost.AddCustomSettings();
            builder.WebHost.AddLogging();

            // var sp = builder.Services.BuildServiceProvider();
            //
            // var settings = sp.GetService<ISettings>();
            // var customSettings = sp.GetService<ICustomSettings>();
            // var logger = sp.GetService<ILogger>();
            //
            // var startup = new Startup(settings, customSettings, logger);
            
            var startup = new Startup(builder.Configuration);

            startup.ConfigureServices(builder.Services);

            // var app = builder.Build();
            //
            // startup.Configure(app, app.Environment, app.Lifetime);

            return builder.WebHost;
        }
        
        protected override void EmptyAggregateRepositories(CancellationToken ct)
        {
            ForecastRepository.DeleteAll(ActionId, CancellationToken.None);
        }

        protected override async Task EmptyAggregateRepositoriesAsync(CancellationToken ct)
        {
            await ForecastRepository.DeleteAllAsync(ActionId, CancellationToken.None);
        }
        
        protected Task EnsureSummariesAsync()
            => new EnsureSummaries(SummaryRepository).ExecuteAsync();
        
        // Actions

        protected PredictWeatherAction PredictWeatherAction => TestServer.Host.Services.GetRequiredService<PredictWeatherAction>();
        protected GetAverageTemperatureAction GetAverageTemperatureAction => TestServer.Host.Services.GetRequiredService<GetAverageTemperatureAction>();
        protected NotifyWeatherPredictedAction NotifyWeatherPredictedAction => TestServer.Host.Services.GetRequiredService<NotifyWeatherPredictedAction>();

        // Settings
        
        protected ICustomSettings CustomSettings => TestServer.Host.Services.GetRequiredService<ICustomSettings>();
        
        // Hooks
        
        protected IOnBeforePrimaryAdaptersStartedHook OnBeforePrimaryAdaptersStartedHook => TestServer.Host.Services.GetRequiredService<IOnBeforePrimaryAdaptersStartedHook>();

        // Repositories
        
        protected IForecastRepository ForecastRepository => TestServer.Host.Services.GetRequiredService<IForecastRepository>();
        protected ISummaryRepository SummaryRepository => TestServer.Host.Services.GetRequiredService<ISummaryRepository>();
        
        // Assertions

        protected void AssertEmailSent(Email toEmail)
            => AssertEmailSent(toEmail: toEmail, msgContains: null);

        protected void AssertEmailSent(Email toEmail, string? msgContains)
            => Assert.True(
                EmailAdapter.HasSent(toEmail: toEmail.ToString(), msgContains: msgContains),
                $"Expected an email{(msgContains != null ? " containing '"+msgContains+"'" : "")} to be sent to {toEmail}.");

        // Execute
        
        protected async Task GetAverageTemperature()
        {
            var command = new GetAverageTemperatureCommand
            {
                
            };
        
            await GetAverageTemperatureAction.ExecuteAsync(command, ActionId, CancellationToken.None);
        }
        
        protected async Task NotifyWeatherPredicted(ForecastId forecastId, DateTime date, int temperatureC, SummaryId summaryId)
        {
            var command = new NotifyWeatherPredictedCommand
            {
                ForecastId = forecastId,
                Date = date,
                TemperatureC = temperatureC,
                SummaryId = summaryId
            };
        
            await NotifyWeatherPredictedAction.ExecuteAsync(command, ActionId, CancellationToken.None);
        }

        protected async Task PredictWeather()
        {
            var command = new PredictWeatherCommand
            {
                
            };
        
            await PredictWeatherAction.ExecuteAsync(command, ActionId, CancellationToken.None);
        }

        // Data

        protected async Task Refresh(Forecast forecast)
        {
            var forecasts = new List<Forecast>();
            foreach (var f in Forecasts)
                if (f.ForecastId == forecast.ForecastId)
                    forecasts.Add(await ForecastRepository.GetAsync(f.ForecastId, ActionId, CancellationToken.None));
                else
                    forecasts.Add(f);
            Forecasts = forecasts;
        }
    }
}
