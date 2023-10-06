# OpenDDD.NET

[![License](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![NuGet](https://img.shields.io/nuget/v/OpenDDD.NET.svg)](https://www.nuget.org/packages/OpenDDD.NET/)

OpenDDD.NET is an open-source framework for domain-driven design (DDD) development using C# and .NET. It provides a set of powerful tools and abstractions to help developers build scalable, maintainable, and testable applications following the principles of DDD.

## Key Features

- **Aggregates**: Define domain aggregates with clear boundaries and encapsulate domain logic within them.
- **Entities and Value Objects**: Create entities and value objects to represent domain concepts and ensure strong type safety.
- **Repositories**: Define repositories to abstract away data access and enable persistence of domain objects.
- **Domain Events**: Implement domain events to facilitate communication between domain objects and decouple them from each other.
- **Integration Events**: Implement integration events to facilitate communication between bounded contexts and decouple them from each other.
- **Application Services**: Use application services to coordinate the execution of domain logic and manage transactions.
- **Dependency Injection**: Leverage the built-in dependency injection support in .NET for easy integration with your application.
- **Testability**: Comes prepared with base test classes for efficient and simple unit testing of actions.
- **Horizontal Scaling**: Seamlessly scale your application horizontally to handle increased traffic and demand.

## Basic Concepts

The map below visually represents the key concepts and their interrelationships.

![Concept Map](https://github.com/runemalm/OpenDDD.NET/blob/develop/concept-map.png)

## Supported Versions

- .NET Core 3.1
- .NET 5
- .NET 6 (upcoming)
- .NET 7 (upcoming)
- .NET 8 (upcoming)

## Getting Started

To get started with OpenDDD.NET, follow these simple steps:

1. **Install the NuGet package**: Use the NuGet package manager or the .NET CLI to add the OpenDDD.NET package to your project.

```bash
dotnet add package OpenDDD.NET
```

2. **Install the project template**: If you want to quickly scaffold new projects using OpenDDD.NET, you can install the OpenDDD.NET project template NuGet package.

```bash
dotnet new install OpenDDD.NET-Templates
```

3. **Create a new project**: Use the OpenDDD.NET project template to create a new project with the necessary files, folder structure, and initial configuration already set up.

```bash
dotnet new opendddnet-sln-netcore31 -n "YOUR_SOLUTION_NAME"
```

4. **Start building your application**: Utilize the power of OpenDDD.NET to build scalable and maintainable applications following the principles of DDD.

## Example Usage

In your `Startup.cs` file, you'll need to register various services and configure the middleware pipeline to set up your project to use the framework.

`In case you don't use project templates`, here's an example of how to manually configure your application in Startup.cs:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Infrastructure.Services.Serialization;
using YourCrawlingContext.Domain.Model.Property;
using YourCrawlingContext.Infrastructure.Ports.Adapters.Site.ThailandProperty;

namespace YourCrawlingContext.Main
{
    public class Startup
    {
        // ... (existing code)

        public void ConfigureServices(IServiceCollection services)
        {
        
            // ... (other service configurations)

            // Event processor  
            services.AddEventProcessorHostedService(Configuration);
            services.AddEventProcessorDatabaseConnection(Configuration);  
            services.AddEventProcessorMessageBrokerConnection(Configuration);  
            services.AddEventProcessorOutbox();  
              
            // Action services
            services.AddActionDatabaseConnection(Configuration);  
            services.AddActionMessageBrokerConnection(Configuration);  
            services.AddActionOutbox();  

            // Actions            
            services.AddAction<CrawlSearchPage.Action, CrawlSearchPage.Command>();  
            services.AddAction<GetProperties.Action, GetProperties.Command>();  
              
            // Publishers  
            services.AddDomainPublisher();  
            services.AddIntegrationPublisher();  
              
            // Listeners  
            services.AddDomainEventListener(SearchPageCrawledListener);  
            services.AddIntegrationEventListener(IcAccountCreatedListener);  
              
            // Repositories  
            services.AddRepository<IPropertyRepository, PropertyRepository>();  
            services.AddRepository<ISiteRepository, SiteRepository>();  
        }

        // ... (existing code)
    }
}
```

**Example:** 
Implement one of the `Actions` that collectively form your `Application Service`:

```csharp
using OpenDDD.Application;  
using OpenDDD.Domain.Model.Event;  
using YourCrawlingContext.Domain.Model;

// ... (other imports)

namespace YourCrawlingContext.Application
{
    public class CrawlSearchPageAction : BaseAction<Command, SearchResults>  
    {  
        private readonly ISiteRepository _siteRepository;  
          
        public CrawlSearchPageAction(  
            IActionDatabaseConnection actionDatabaseConnection,  
            IActionOutbox outbox,  
            ISiteRepository siteRepository,  
            ISomeWebSitePort someWebSiteAdapter,
            IDomainPublisher domainPublisher,  
            ILogger<CrawlSearchPageAction> logger)  
            : base(actionDatabaseConnection, outbox, someWebSiteAdapter, domainPublisher, logger)  
        {  
            _siteRepository = siteRepository;  
        }

        public override async Task<SearchResults> ExecuteAsync(Command command, ActionId actionId, CancellationToken ct)  
        {  
            var site = await GetAggregateOrThrowAsync(command.SiteId, _siteRepository, actionId, ct);  
              
            var siteAdapter = GetSiteAdapterOrThrow(command.SiteId);  
              
            var searchResults = await site.CrawlSearchPageAsync(siteAdapter, _domainPublisher, actionId, ct);  
              
            await _siteRepository.SaveAsync(site, actionId, ct);  
              
            return searchResults;  
        }
    }
}
```

**Example:** 
Publish an `Domain Event` from an `Aggregate Root`:

```csharp
using OpenDDD.Application;  
using OpenDDD.Domain.Model.AggregateRoot;  
using OpenDDD.Domain.Model.Entity;
using OpenDDD.Domain.Model.Event;

// ... (other imports)
  
namespace YourCrawlingContext.Domain.Model.Site  
{  
    public class Site : AggregateRoot, IAggregateRoot, IEquatable<Site>  
    {  
        public SiteId SiteId { get; set; }    
  
        public string Name { get; set; }  
  
        // ... (other methods)

        public async Task<SearchResults.SearchResults> CrawlSearchPageAsync(ISitePort siteAdapter, IDomainPublisher domainPublisher, ActionId actionId, CancellationToken ct)  
        {  
            var searchResults = await siteAdapter.CrawlSearchPageAsync(ct);  
  
            await domainPublisher.PublishAsync(new SearchPageCrawled(SiteId, searchResults, actionId));  
  
            return searchResults;  
        }

        // ... (other methods)
    }
}
```

## Release History

- **v1.0.0-alpha.1** (2022-10-02): Initial alpha release.
- **v1.0.0-alpha.2** (2022-10-09): Make the hexagonal architecture pattern more represented in the namespaces.
- **v1.0.0-alpha.3** (2022-11-20): Refactor JwtToken and add IdToken.

For a complete list of releases and their changelogs, please visit the [Releases](https://github.com/runemalm/OpenDDD.NET/releases) page.

**Note**: We have just released an alpha version of major version 2 of this framework, and some features are still under development. Avoid using it in production environments and expect some code to not be implemented still.

## Contributing

We welcome contributions from the community. To contribute to OpenDDD.NET, please follow these steps:

1. Fork the repository and clone it to your local machine.
2. Create a new branch for your feature or bug fix.
3. Implement your changes and ensure that the existing tests pass.
4. Write new tests to cover your changes and make sure they pass as well.
5. Commit your changes and push them to your fork.
6. Submit a pull request with a clear description of your changes and the problem they solve.

Please make sure to review our [Contributing Guidelines](https://github.com/runemalm/OpenDDD.NET/blob/master/CONTRIBUTING.md) before submitting a pull request.

## License

OpenDDD.NET is licensed under the [GPLv3 License](https://www.gnu.org/licenses/gpl-3.0.html). Feel free to use it in your own projects.

## Acknowledgements

OpenDDD.NET is inspired by the principles and ideas of Domain-Driven Design (DDD) and the fantastic work done by the DDD community. We would like to thank all the contributors and supporters who have helped make this project possible.

## Get in Touch

If you have any questions, suggestions, or feedback, please don't hesitate to reach out to us. You can find more information and ways to contact us on our [Website](https://www.openddd.net).

Let's build better software together with OpenDDD.NET!
