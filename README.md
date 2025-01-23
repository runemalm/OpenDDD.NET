# OpenDDD.NET

[![License](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![NuGet](https://img.shields.io/nuget/v/OpenDDD.NET.svg)](https://www.nuget.org/packages/OpenDDD.NET/)

OpenDDD.NET is an open-source framework for domain-driven design (DDD) development using C# and .NET. It provides a set of powerful tools and abstractions to help developers build scalable, maintainable, and testable applications following the principles of DDD.

> **Note:** OpenDDD.NET is currently in an alpha state as part of a complete rewrite for version 3. While it introduces foundational concepts and aims to provide a production-ready framework eventually, some features are still under development. Use with caution in production environments and consider contributing to its development.

## Key Features

- [x] **Aggregates**: Define domain aggregates with clear boundaries and encapsulate domain logic within them.
- [x] **Entities and Value Objects**: Create entities and value objects to represent domain concepts and ensure strong type safety.
- [x] **Repositories**: Define repositories to abstract away data access and enable persistence of domain objects.
- [ ] **Domain Events**: Implement domain events to facilitate communication between domain objects and decouple them from each other.
- [ ] **Integration Events**: Implement integration events to facilitate communication between bounded contexts and decouple them from each other.
- [ ] **Event Listeners**: Support for defining and managing event listeners to handle domain and integration events, enabling decoupled and scalable event-driven architectures.
- [x] **Domain Services**: Encapsulate domain-specific operations that don’t naturally belong to an entity or value object.
- [x] **Application Services**: Use Action classes to coordinate the execution of domain logic in response to commands.
- [ ] **Infrastructure Services**: Provide implementations for technical concerns such as logging, email, or external integrations.
- [ ] **Transactional Outbox**: Ensure event consistency by persisting and publishing events as part of database transactions.

## Basic Concepts

We're adhering to the key principles and building blocks of Domain-Driven Design.

<img src="https://github.com/runemalm/OpenDDD.NET/blob/master/ddd-graph.png" width="636" alt="DDD Concepts Graph" />

## Supported Versions

- .NET 8

## Sample Project

The `Bookstore` sample project demonstrates how to use OpenDDD.NET in a real-world scenario, including domain modeling, repositories, actions, and framework configuration. 

Explore the project in the repository: [Bookstore Sample Project](https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore).

## Getting Started

To get started with OpenDDD.NET, follow these simple steps:

1. **Install the NuGet package**: Use the NuGet package manager or the .NET CLI to add the OpenDDD.NET package to your project.

```bash
dotnet add package OpenDDD.NET --prerelease
```

2. **Create a new project**: Create a new project in your editor or IDE of choice or use the command below.

```bash
dotnet new webapi -n YourProjectName
```

3. **Start building your application**: Utilize the power of OpenDDD.NET to build scalable and maintainable applications following the principles of DDD.

## Configuration in `appsettings.json`

OpenDDD.NET can be configured using the `appsettings.json` file. Below is an example configuration:

```
{
  "OpenDDD": {  
    "ConnectionString": "",
    "AutoRegisterDomainServices": true,  
    "AutoRegisterRepositories": true,  
    "AutoRegisterActions": true,  
    "PersistenceProvider": "InMemory"  
  }
}
```

### Explanation of Configuration Options

- **`ConnectionString`**: Specifies the connection string for persistence providers (e.g., databases). This is required for certain providers like `Postgres`.

- **`AutoRegisterDomainServices`**: Automatically registers all domain service interfaces (e.g. `ICustomerDomainService`) with their corresponding implementations.

- **`AutoRegisterRepositories`**: Automatically registers all repository interfaces (e.g., `ICustomerRepository`) with their corresponding implementations.

- **`AutoRegisterActions`**: Automatically registers all classes implementing `IAction<TCommand, TReturns>`.

- **`PersistenceProvider`**: Defines the persistence technology to use (e.g., `InMemory`, `Postgres`). Determines the implementation used for repositories and other persistence-related features.

These options can be customized in `appsettings.json` or overridden programmatically in `Program.cs`.

## Example Usage

In your `Program.cs` file, you'll need to register various services and configure the middleware pipeline to set up your project to use the framework.

Here's an example of how to manually configure your application in Program.cs:

```csharp
using OpenDDD.Main.Extensions;  
  
var builder = WebApplication.CreateBuilder(args);  
  
// ...
  
// Add OpenDDD Services
builder.Services.AddOpenDDD(builder.Configuration);  
  
// ...

var app = builder.Build();
  
// Use OpenDDD Middleware
app.UseOpenDDD();  
  
// ...

app.Run();
```

With this setup, you can begin implementing domain-driven design principles using OpenDDD.NET in your application.

## Documentation

Comprehensive documentation for OpenDDD.NET is available on **Read the Docs**. The documentation includes guides, examples, and reference materials to help you get started and make the most of the framework.

Visit the documentation here: [OpenDDD.NET Documentation](https://opendddnet.readthedocs.io/)

## Release History

**3.0.0-alpha.2 (2025-01-21)**

- **Domain Services:** Added the `IDomainService` interface to mark domain services, which are now auto-registered.
- **Repositories:** Introduced support for repositories with `InMemoryRepositoryBase` as the base class for custom implementations and thus first supported persistence provider.
- **Actions:** Added the `IAction<TCommand, TReturns>` interface to mark actions, which are now auto-registered.
- **Sample Project:** Introduced the `Bookstore` sample project to demonstrate OpenDDD.NET usage.
- **Documentation Updates:** Expanded documentation with examples and guides for repositories, domain services, actions and the sample project.
- **Community:** Added an advanced topics section to the documentation, covering concepts useful for contributors or developers looking to extend the framework’s functionality.

**3.0.0-alpha.1 (2025-01-19)**

- **Major Version 3**: Initial alpha release with foundational features.

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
