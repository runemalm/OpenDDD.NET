# OpenDDD.NET

[![License](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![NuGet](https://img.shields.io/nuget/v/OpenDDD.NET.svg)](https://www.nuget.org/packages/OpenDDD.NET/)

OpenDDD.NET is an open-source framework for domain-driven design (DDD) development using C# and .NET. It provides a set of powerful tools and abstractions to help developers build scalable, maintainable, and testable applications following the principles of DDD.

**Note:** This project is currently in its alpha stage of development. While we strive to provide a comprehensive experience, please be aware of the following:

- Not all links or features may be fully functional at this stage.
- Some code sections may still be in progress or incomplete.

We appreciate your understanding as we work towards further enhancing and stabilizing the project. Your feedback and contributions are invaluable in helping us reach a more polished state.


## Key Features

- **Aggregates**: Define domain aggregates with clear boundaries and encapsulate domain logic within them.
- **Entities and Value Objects**: Create entities and value objects to represent domain concepts and ensure strong type safety.
- **Repositories**: Define repositories to abstract away data access and enable persistence of domain objects.
- **Domain Events**: Implement domain events to facilitate communication between domain objects and decouple them from each other.
- **Integration Events**: Implement integration events to facilitate communication between bounded contexts and decouple them from each other.
- **Application Services**: Use application services to coordinate the execution of domain logic and manage transactions.
- **Dependency Injection**: Leverage the built-in dependency injection support in .NET for easy integration with your application.
- **Testing**: Use base test classes for efficient and powerful unit testing of actions.

## Basic Concepts

The map below visually represents the key concepts and their interrelationships in a clear and concise manner.

![Concept Map](https://github.com/runemalm/OpenDDD.NET/blob/develop/concept-map.png)

## Supported Versions

- .NET Core 3.1

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
dotnet new openddd-net -n "YOUR_SOLUTION_NAME"
```

4. **Start building your application**: Utilize the power of OpenDDD.NET to build scalable and maintainable applications following the principles of DDD.

For detailed documentation and code examples, please refer to the [User Guide](https://opendddnet.readthedocs.io/en/latest/index.html) of this repository.

## Example Code

Sample code can be discovered within the project templates detailed above. Additionally, we will provide links to other applications and projects that utilize OpenDDD.NET as they are developed and shared by the community.

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
