# OpenDDD.NET

[![License](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![NuGet](https://img.shields.io/nuget/v/OpenDDD.NET.svg)](https://www.nuget.org/packages/OpenDDD.NET/)

OpenDDD.NET is an open-source framework for domain-driven design (DDD) development using C# and .NET. It provides a set of powerful tools and abstractions to help developers build scalable, maintainable, and testable applications following the principles of DDD.

The framework follows .NET conventions for seamless integration, ensuring compatibility with standardized naming, dependency injection patterns, and middleware setup. It is also designed for non-intrusive adoption, enabling developers to try out its features in existing codebases without significant refactoring.

## Key Features

- [x] **Aggregates**: Define domain aggregates with clear boundaries and encapsulate domain logic within them.
- [x] **Entities and Value Objects**: Create entities and value objects to represent domain concepts and ensure strong type safety.
- [x] **Repositories**: Define repositories to abstract away data access and enable persistence of domain objects.
- [ ] **Domain Events**: Implement domain events to facilitate communication between domain objects and decouple them from each other.
- [ ] **Integration Events**: Implement integration events to facilitate communication between bounded contexts and decouple them from each other.
- [ ] **Event Listeners**: Support for defining and managing event listeners to handle domain and integration events, enabling decoupled and scalable event-driven architectures.
- [x] **Application Services**: Use application services and actions to coordinate the execution of domain logic and manage transactions.
- [x] **Domain Services**: Encapsulate domain-specific operations that don’t naturally belong to an entity or value object.
- [x] **Infrastructure Services**: Provide implementations for technical concerns such as logging, email, or external integrations.
- [ ] **Transactional Outbox**: Ensure event consistency by persisting and publishing events as part of database transactions.

## Basic Concepts

We're adhering to the key principles and building blocks of Domain-Driven Design.

![DDD Concept Graph](https://github.com/runemalm/OpenDDD.NET/blob/develop/ddd-graph.png)

## Supported Versions

- .NET 6 (not tested)
- .NET 7 (not tested)
- .NET 8

## Getting Started

To get started with OpenDDD.NET, follow these simple steps:

1. **Install the NuGet package**: Use the NuGet package manager or the .NET CLI to add the OpenDDD.NET package to your project.

```bash
dotnet add package OpenDDD.NET
```

2. **Create a new project**: Create a new project in your editor or IDE of choice or use the command below.

```bash
dotnet new webapi -n YourProjectName
```

3. **Start building your application**: Utilize the power of OpenDDD.NET to build scalable and maintainable applications following the principles of DDD.

## Example Usage

In your `Program.cs` file, you'll need to register various services and configure the middleware pipeline to set up your project to use the framework.

Here's an example of how to manually configure your application in Program.cs:

```csharp
using OpenDDD;

var builder = WebApplication.CreateBuilder(args);

// Register OpenDDD services
builder.Services.AddOpenDDD(options =>
{
    options.UseDomainEvents();
    options.UseRepositories();
    options.UseApplicationServices();
    // Add additional configurations here
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

With this setup, you can begin implementing domain-driven design principles using OpenDDD.NET in your application.

## Release History

- **v3.0.0-alpha.1** (2025-01-xx): Initial alpha release introducing foundational concepts.

For a complete list of releases and their changelogs, please visit the [Releases](https://github.com/runemalm/OpenDDD.NET/releases) page.

**Note**: Early adopters know that we have released a major version 1 and 2. We are now changing direction and have completely redesigned the framework. Star and follow the repository to follow our progression towards a stable major 3 release. You can see the feature list below which concepts we have left to add before release.

## Contributing

We welcome contributions from the community. To contribute to OpenDDD.NET, please follow these steps:

1. Fork the repository and clone it to your local machine.
2. Create a new branch for your feature or bug fix.
3. Implement your changes and ensure that the existing tests pass.
4. Write new tests to cover your changes and make sure they pass as well.
5. Commit your changes and push them to your fork.
6. Submit a pull request on the develop branch with a clear description of your changes and the problem they solve.

Please make sure to review our [Contributing Guidelines](https://github.com/runemalm/OpenDDD.NET/blob/master/CONTRIBUTING.md) before submitting a pull request.

## License

OpenDDD.NET is licensed under the [GPLv3 License](https://www.gnu.org/licenses/gpl-3.0.html). Feel free to use it in your own projects.

## Acknowledgements

OpenDDD.NET is inspired by the principles and ideas of Domain-Driven Design (DDD) and the fantastic work done by the DDD community. We would like to thank all the contributors and supporters who have helped make this project possible.

## Get in Touch

If you have any questions, suggestions, or feedback, please don't hesitate to reach out to us.

Let's build better software together with OpenDDD.NET!
