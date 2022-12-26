## DDD.NETCore

This is a framework for domain-driven design (DDD) development with C# and .NET Core.

Star and/or follow the project to don't miss notifications of upcoming releases.

### Goal

The goal with this project is to be able to do DDD with .NET Core.

We couldn't find an existing framework with the features that we required. We wanted to be able to autonomously refine and implement the domain model, without worrying about dependencies on frontend teams and other third-party clients.

Another requirement was that it should be possible to build "near-infinitely scalable" applications, based on the *Entity* pattern.

We also really like the hexagonal architecture pattern and how a software system becomes easier to understand when it's used.

And so this framework was born.

### Key Features

- Domain model versioning.
- Fully autonomous development.
- Auto-generated API documentation.
- Backwards-compatible API support.
- On-the-fly migration.
- Recommended workflow.

### Design Patterns

The framework is based on the following design patterns:

- [Domain-Driven Design](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)  
- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Event-Carried State Transfer](https://martinfowler.com/articles/201701-event-driven.html)
- [Near-infinite Scalability](https://queue.acm.org/detail.cfm?id=3025012)
- [xUnit](https://en.wikipedia.org/wiki/XUnit)
- [Expand and Contract](https://martinfowler.com/bliki/ParallelChange.html)
- [Env files](https://12factor.net/config)

Credits to Eric Evans for his [seminal book on DDD](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215) and 
Vaughn Vernon for his [reference implementation](https://github.com/VaughnVernon/IDDD_Samples) of DDD in Java.

### Supported versions

- .NET 7 (not tested)
- .NET 6.0
- .NET 5.0 (not tested)
- .NET Core 3.1
  
### Installation

    Install-Package DDD.NETCore

### Example

These files below are from the [WeatherForecast]() sample project.

```c#
// Program.cs

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

# code removed in favour of brevity ...

services.AddAction<PredictWeatherAction, PredictWeatherCommand>();
services.AddListener<WeatherPredictedListener>();
services.AddRepository<IForecastRepository, PostgresForecastRepository>();

# code removed in favour of brevity ...
```

```c#
// Forecast.cs

namespace Domain.Model.Forecast  
{  
  public class Forecast : Aggregate, IAggregate, IEquatable<Forecast>  
  {  
        public ForecastId ForecastId { get; set; }  
        EntityId IAggregate.Id => ForecastId;  
          
        public DateTime Date { get; set; }  
        public int TemperatureC { get; set; }  
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);  
        public string Summary { get; set; }  
  
        // Public  
  
        public static async Task<Forecast> PredictTomorrow(  
            ForecastId forecastId,   
            ActionId actionId,  
            IDomainPublisher domainPublisher,  
            IInterchangePublisher interchangePublisher,  
            IIcForecastTranslator icForecastTranslator)  
        {  
            var forecast =  
                new Forecast()  
                {  
                    DomainModelVersion = Domain.Model.DomainModelVersion.Latest(),  
                    ForecastId = forecastId,  
                    Date = DateTime.Now.AddDays(1),  
                    TemperatureC = Random.Shared.Next(-20, 55),  
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]  
                };  
  
            forecast.Validate();  
              
            await domainPublisher.PublishAsync(new WeatherPredicted(forecast, actionId));  
            await interchangePublisher.PublishAsync(new IcWeatherPredicted(icForecastTranslator.To(forecast), actionId));  
  
            return forecast;  
        }

        # code removed in favour of brevity ...
  }
}
```

```bash
# env.local.vs

# Logging
CFG_LOGGING_LEVEL_DOTNET=Information
CFG_LOGGING_LEVEL=Debug

# General
CFG_GENERAL_CONTEXT=Weather

# Auth
CFG_AUTH_ENABLED=false
CFG_AUTH_RBAC_PROVIDER=PowerIAM
CFG_AUTH_RBAC_EXTERNAL_REALM_ID=some-external-id
CFG_AUTH_JWT_TOKEN_PRIVATE_KEY=some-fake-private-key
CFG_AUTH_JWT_TOKEN_NAME=Authorization
CFG_AUTH_JWT_TOKEN_LOCATION=header
CFG_AUTH_JWT_TOKEN_SCHEME=Bearer

# Http Adapter
CFG_HTTP_URLS=http://localhost:9000
CFG_HTTP_CORS_ALLOWED_ORIGINS=https://localhost:5052,http://localhost:5051
CFG_HTTP_DOCS_DEFINITIONS=Public,Public,
CFG_HTTP_DOCS_ENABLED=true
CFG_HTTP_DOCS_HTTP_ENABLED=true
CFG_HTTP_DOCS_HTTPS_ENABLED=false
CFG_HTTP_DOCS_HOSTNAME=localhost:5051
CFG_HTTP_DOCS_AUTH_EXTRA_TOKENS=
CFG_HTTP_DOCS_TITLE=Weather API

# Persistence
CFG_PERSISTENCE_PROVIDER=Postgres
CFG_PERSISTENCE_POOLING_ENABLED=true
CFG_PERSISTENCE_POOLING_MIN_SIZE=0
CFG_PERSISTENCE_POOLING_MAX_SIZE=100

# Postgres
CFG_POSTGRES_CONN_STR="Host=localhost:9092;Username=net60;Password=net60;Database=net60"

# PubSub
CFG_PUBSUB_PROVIDER=Rabbit
CFG_PUBSUB_MAX_DELIVERY_RETRIES=3
CFG_PUBSUB_PUBLISHER_ENABLED=true

# Monitoring
CFG_MONITORING_PROVIDER=AppInsights

# PowerIAM
CFG_POWERIAM_URL=http://localhost:9000

# Service Bus
CFG_SERVICEBUS_CONN_STR=
CFG_SERVICEBUS_SUB_NAME=

# Rabbit
CFG_RABBIT_HOST=localhost
CFG_RABBIT_PORT=5672
CFG_RABBIT_USERNAME=guest
CFG_RABBIT_PASSWORD=guest

# Email
CFG_EMAIL_ENABLED=true
CFG_EMAIL_PROVIDER=smtp
CFG_EMAIL_SMTP_HOST=localhost
CFG_EMAIL_SMTP_PORT=1025
```

### Documentation:
  
Documentation is coming in v1.0.0 rc.

### Semantic versioning in HTTP adapters ("APIs")

DDD.NETCore requires you to start http adapter (api) versions at  `1.0.0`  and increment as follows:

| Code Status                               | Stage         | Rule                                                               | Example Version |
|-------------------------------------------|---------------|--------------------------------------------------------------------|-----------------|
| First release                             | New product   | Start with 1.0.0                                                   | 1.0.0           |
| Backward-compatible bug fixes             | Patch release | Increment the third digit                                          | 1.0.1           |
| Backward compatible new features          | Minor release | Increment the middle digit and reset last digit to zero            | 1.1.0           |
| Changes that break backward compatibility | Major release | Increment the first digit and reset middle and last digits to zero | 2.0.0           |

In addition, you can utilise `alpha`, `beta` and `rc` in your version numbers to specify pre-release, beta and release candidates.

### Contribution:
  
If you want to contribute to the code base, create a pull request on the develop branch.

### Roadmap v1.0.0:

- [ ] GitHub README
- [ ] NuGet README
- [ ] Visual Studio Project Template .NET 7
- [x] Visual Studio Project Template .NET 6.0
- [ ] Visual Studio Project Template .NET 5.0
- [ ] Visual Studio Project Template .NET Core 3.1
- [x] Start Context
- [x] Stop Context
- [x] Control
- [x] On-the-fly aggregate migration
- [x] Auto-code Generation from domain.yml File
- [x] Postgres Dead Letter Queue
- [x] Memory Dead Letter Queue
- [x] Dead Letter Queue
- [x] Handle Poisonous Messages
- [x] Re-publish Failed Events
- [x] Postgres Outbox
- [x] Memory Outbox
- [x] Outbox
- [x] Domain Event Publishing
- [x] Integration Event Publishing
- [x] Rabbit Event Adapter
- [x] Memory Event Adapter
- [x] PubSub
- [x] Auth Domain Service
- [x] Auth
- [x] Aggregate
- [x] Entity
- [x] Value Object
- [x] Domain Event
- [x] Integration Event
- [x] Repository
- [x] Building Blocks
- [x] Domain Service
- [x] Infrastructure Service
- [x] Application Service
- [x] Auto-Generated Swagger HTTP Adapter Documentation
- [x] HTTP Adapter
- [x] Email Adapter
- [x] Persistence Service
- [x] Postgres Repository
- [x] Memory Repository

### Roadmap Future:

- [ ] Full Sample Project
- [ ] Quickstart Guide
- [ ] Documentation
- [ ] Azure Monitoring Adapter.
- [ ] Monitoring
- [ ] Task: Migrate all aggregate roots
- [ ] Tasks
- [ ] All-aggregates Migration Task
- [ ] Periodic Jobs
- [ ] Interval Jobs
- [ ] One-off Jobs
- [ ] Jobs
- [ ] Merge old API versions into current swagger json files.
- [ ] CLI Operation: Create action/aggregate/event/...
- [ ] CLI Operation: Migrate
- [ ] CLI Operation: Create Migration
- [ ] CLI Operation: Increment Domain Model Version
- [ ] Test Framework
- [ ] Tests
- [ ] Admin Dashboard
- [ ] Admin Tool: Inspect Dead Event
- [ ] Admin Tool: Republish Dead Event
- [ ] Administration

### Release Notes:

**1.0.0-alpha.4** - 2022-12-10

- Add configuration setting for which server urls to listen to. (**breaking**)
- Fix concurrency issues with memory repositories.
- Add support for IAM ports.
- Add 'PowerIAM' adapter.
- Add RBAC auth settings. (**breaking**)
- Add a base 'Migrator' class. (**breaking**)

**1.0.0-alpha.3** - 2022-11-20

- Refactor JwtToken and add IdToken. (**breaking**)
- Add more tasks to code generation tool.
- Add support for http put methods to code generation tool.
- Add some missing repository method implementations.
- Add GetAsync(IEnumerable<...> ...) to repositories.
- Add convenience methods to ApplicationExtensions.
- Return 400 http status code on domain- and invariant exceptions in primary http adapter.

**1.0.0-alpha.2** - 2022-10-09

- Make the hexagonal architecture more represented in the namespaces.
 
**1.0.0-alpha.1** - 2022-10-02

This is the first (alpha) release of the framework.
Please try it out and submit tickets or otherwise reach out if you find any issues or have any questions.

**0.9.0-alpha7** - 2022-07-31

First alpha release on nuget.org.
