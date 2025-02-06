# OpenDDD.NET

[![License](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![NuGet](https://img.shields.io/nuget/v/OpenDDD.NET.svg)](https://www.nuget.org/packages/OpenDDD.NET/)

OpenDDD.NET is an open-source framework for domain-driven design (DDD) development using C# and .NET. It provides a set of powerful tools and abstractions to help developers build scalable, maintainable, and testable applications following the principles of DDD.

> **Note:** OpenDDD.NET is currently in an alpha state as part of new major version 3. Use with caution in production environments.

⭐ Consider **starring** and/or **following** the project to stay updated with the latest developments.

## Key Features

- **Aggregates**: Define domain aggregates with clear boundaries and encapsulate domain logic within them.
- **Entities and Value Objects**: Create entities and value objects to represent domain concepts and ensure strong type safety.
- **Repositories**: Abstract away data access and enable persistence of domain objects.
- **Domain Events**: Facilitate communication between domain objects while maintaining loose coupling.
- **Integration Events**: Enable communication between bounded contexts in distributed systems.
- **Event Listeners**: Manage event listeners to handle domain and integration events for scalable, event-driven architectures.
- **Domain Services**: Encapsulate domain-specific operations that do not naturally belong to an entity or value object.
- **Application Services**: Use Action classes to coordinate the execution of domain logic in response to commands.
- **Infrastructure Services**: Provide implementations for technical concerns such as logging, email, or external integrations.
- **Transactional Outbox**: Ensure event consistency by persisting and publishing events as part of database transactions.

We're adhering to the key principles and building blocks of Domain-Driven Design.

<img src="https://github.com/runemalm/OpenDDD.NET/blob/master/ddd-graph.png" width="636" alt="DDD Concepts Graph" />

## Supported Versions

- ASP.NET Core 8

## Getting Started

To get started with OpenDDD.NET, follow these simple steps:

1. **Install the NuGet package**: Use the NuGet package manager or the .NET CLI to add the OpenDDD.NET package to your project.

   ```bash
   dotnet add package OpenDDD.NET --prerelease
   ```

2. **Create a new project**: Create a new project in your editor or IDE of choice or use the command below.

   ```bash
   dotnet new webapi -n Bookstore
   ```

3. **Set up OpenDDD.NET**: Register OpenDDD services and middleware in your `Program.cs` file.

   ```csharp
   using OpenDDD.API.Extensions;

   var builder = WebApplication.CreateBuilder(args);

   // Add OpenDDD services
   builder.Services.AddOpenDDD<BookstoreDbContext>(builder.Configuration, options =>  
   {  
       options.UseEfCore()
              .UseSQLite("DataSource=Main/EfCore/Bookstore.db;Cache=Shared")
              .UseInMemoryMessaging()
              .SetEventListenerGroup("Default")  
              .SetEventTopicTemplates(  
                 "Bookstore.Domain.{EventName}",  
                 "Bookstore.Interchange.{EventName}"  
              )
              .EnableAutoRegistration();
   });

   var app = builder.Build();

   // Use OpenDDD Middleware
   app.UseOpenDDD();

   app.Run();
   ```

4. **Start building your application**: Utilize the power of OpenDDD.NET to build scalable and maintainable applications.

For detailed guides and examples, refer to the documentation.


## Documentation

The official [OpenDDD.NET Documentation](https://opendddnet.readthedocs.io/) provides getting-started guide, examples, and configuration references to help you get started and make the most of the framework.  

## Sample Project

The `Bookstore` sample project demonstrates how to use OpenDDD.NET in a real-world scenario, including domain modeling, repositories, actions, and framework configuration. 

Explore the project in the repository: [Bookstore Sample Project](https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore).

## Release History

**3.0.0-alpha.4 (2025-02-xx)**

- **Messaging Providers**: Fix some issues with the `Azure Service Bus` provider.
- **Seeders**: Add support for seeders to seed aggregates on application start.
- **Namespace**: Change the name of the namespace `Main` -> `API`.
- **Project Template**: Add a project template nuget for quick scaffolding of a new project.
- **Documentation**: Refactor the documentation to reflect new changes and improve onboarding experience.

**3.0.0-alpha.3 (2025-01-30)**

- **Domain Events**: Added support for domain events to enable communication between domain objects while maintaining encapsulation.
- **Integration Events**: Added support for integration events to facilitate communication between bounded contexts.
- **Event Listeners**: Added support for event listeners to handle domain and integration events with actions.
- **Transactional Outbox**: Added reliable event publishing by storing events in an outbox before processing and publishing to the message bus, ensuring consistency with database transactions.
- **Messaging Providers**: Added pluggable support for messaging backends, including in-memory and Azure Service Bus.
- **Infrastructure Services**: Added `IInfrastructureService` interface for managing infrastructure services, with automatic registration.
- **Repository Refactoring**: Refactored repository pattern by introducing `EfCoreRepository<TAggregate, TId>` as the default for the EfCore persistence provider, improving consistency and customization.
- **Configuration Refactoring**: Restructured OpenDDD.NET configuration system into hierarchical options classes, improving clarity and maintainability.

For a complete list of releases and their changelogs, please visit the [Releases](https://github.com/runemalm/OpenDDD.NET/releases) page.

## Contributing

We welcome contributions from the community. To contribute to OpenDDD.NET, please follow these steps:

1. Fork the repository on GitHub.
2. Clone your forked repository to your local machine.
3. Create a new branch from the `master` branch for your changes.
4. Make your modifications and ensure they adhere to our coding conventions.
5. Write appropriate tests for your changes, ensuring they pass.
6. Commit your changes with a descriptive and meaningful commit message.
7. Push your branch to your forked repository on GitHub.
8. Open a pull request (PR) against the `develop` branch of the main repository.

Please make sure to review our [Contributing Guidelines](https://github.com/runemalm/OpenDDD.NET/blob/master/CONTRIBUTING.md) before submitting a pull request.

## License

OpenDDD.NET is licensed under the [GPLv3 License](https://www.gnu.org/licenses/gpl-3.0.html). Feel free to use it in your own projects.

## Acknowledgements

OpenDDD.NET is inspired by the principles and ideas of Domain-Driven Design (DDD) and the fantastic work done by the DDD community. We would like to thank all the contributors and supporters who have helped make this project possible.

## Get in Touch

If you have any questions, suggestions, or feedback, please don't hesitate to reach out to us.

Let's build better software together with OpenDDD.NET!
