# OpenDDD.NET

[![License](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![NuGet](https://img.shields.io/nuget/v/OpenDDD.NET.svg)](https://www.nuget.org/packages/OpenDDD.NET/)

OpenDDD.NET is an open-source framework for domain-driven design (DDD) development using C# and .NET Core. It provides a set of powerful tools and abstractions to help developers build scalable, maintainable, and testable applications following the principles of DDD.

## Key Features

- **Aggregates**: Define domain aggregates with clear boundaries and encapsulate domain logic within them.
- **Entities and Value Objects**: Create entities and value objects to represent domain concepts and ensure strong type safety.
- **Repositories**: Define repositories to abstract away data access and enable persistence of domain objects.
- **Domain Events**: Implement domain events to facilitate communication between domain objects and decouple them from each other.
- **Application Services**: Use application services to coordinate the execution of domain logic and manage transactions.
- **Dependency Injection**: Leverage the built-in dependency injection support in .NET Core for easy integration with your application.
- **Testability**: OpenDDD.NET promotes testability by providing interfaces and abstractions that allow for easy mocking and unit testing.
- **Hexagonal Architecture**: OpenDDD.NET is based on the hexagonal architecture, promoting a clear separation between the core domain and external dependencies.
- **Expand and Contract**: The framework follows the expand and contract design pattern, making it easy to add new features without modifying existing code.
- **Dotenv**: OpenDDD.NET supports the dotenv pattern for managing configuration and environment-specific settings.

## Supported .NET Versions

- .NET Core 3.1
- .NET 5

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
dotnet new openddd-net -n "MyContext"
```

4. **Define your domain model**: Create domain aggregates, entities, value objects, and repositories according to your domain requirements.
5. **Implement domain logic**: Encapsulate your domain logic within aggregates and entities, utilizing domain events when necessary.
6. **Use application services**: Create application services to orchestrate the execution of domain logic and manage transactions.
7. **Wire up dependencies**: Use the built-in dependency injection support in .NET Core to wire up your domain objects and application services.
8. **Start building your application**: Utilize the power of OpenDDD.NET to build scalable and maintainable applications following the principles of DDD.

For detailed documentation and code examples, please refer to the [User Guide](https://opendddnet.readthedocs.io/en/latest/index.html) of this repository.

## Example Code

You can find example code and usage scenarios in the [PowerIAM](https://github.com/poweriam) project, which is an open-source identity and access management system built using OpenDDD.NET.

## Release History

- **v1.0.0-alpha.1** (2022-10-02): Initial alpha release.
- **v1.0.0-alpha.2** (2022-10-09): Make the hexagonal architecture pattern more represented in the namespaces.
- **v1.0.0-alpha.3** (2022-11-20): Refactor JwtToken and add IdToken.

For a complete list of releases and their changelogs, please visit the [Releases](https://github.com/runemalm/OpenDDD.NET/releases) page.

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
