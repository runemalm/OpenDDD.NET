[![License](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html) [![NuGet](https://img.shields.io/nuget/v/OpenDDD.NET.svg)](https://www.nuget.org/packages/OpenDDD.NET/) [![Documentation](https://img.shields.io/badge/docs-Read%20the%20Docs-blue.svg)](https://openddd.net) [![Downloads](https://img.shields.io/nuget/dt/OpenDDD.NET.svg)](https://www.nuget.org/packages/OpenDDD/) [![Tests](https://github.com/runemalm/OpenDDD.NET/actions/workflows/tests.yml/badge.svg?branch=master)](https://github.com/runemalm/OpenDDD.NET/actions/workflows/tests.yml)


# OpenDDD.NET

**OpenDDD.NET** is an opinionated framework for building **Domain-Driven Design (DDD)** applications on ASP.NET Core.

It lets you focus on modeling your aggregates, repositories, domain services, and domain events — while the framework handles persistence, messaging, and transactional consistency.

> **Focus on your domain model. Forget the plumbing.**

---

## Key Features

- **Aggregate Roots, Entities, Value Objects** — Built-in base classes for modeling your domain.
- **Domain Services** — For domain logic that doesn’t naturally fit inside an aggregate.
- **Domain Events & Integration Events** — Publish events inside and outside the boundary of your service.
- **Repository Abstractions** — Works with PostgreSQL, SQL Server, SQLite, EF Core, and InMemory.
- **Messaging Abstraction** — Supports RabbitMQ, Azure Service Bus, Kafka, and InMemory.
- **Transactional Outbox** — Guarantees reliable and atomic event publishing.
- **Automatic Registration** — The framework automatically registers repositories, domain services, actions, event listeners, and infrastructure services based on conventions.
- **Drop-in Setup** — Configure once in `Program.cs` and `appsettings.json`, and you're ready to go.
- **Opinionated but Flexible** — Use convention-over-configuration or break out and compose manually.

---

## Why OpenDDD.NET?

OpenDDD.NET is for developers who want to focus on the **domain model** — aggregate roots, domain services, and domain events — without dealing with infrastructure setup.

- **Batteries included.**
- **Minimal setup.** 
- **Minimal boilerplate.**

---

## 🚀 Quick Start

### Install via NuGet

```bash
dotnet add package OpenDDD.NET --prerelease
```

---

### Minimal setup in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add OpenDDD services and configure 
builder.Services.AddOpenDDD(builder.Configuration);

var app = builder.Build();

// Add OpenDDD middleware
app.UseOpenDDD();

app.Run();
```

---

### Configure in `appsettings.json`

```json
{
  "OpenDDD": {
    "PersistenceProvider": "OpenDdd",
    "DatabaseProvider": "Postgres",
    "Postgres": {
      "ConnectionString": "Host=localhost;Port=5432;Database=bookstore;Username=postgres;Password=password"  
    },
    "MessagingProvider": "RabbitMq",
    "RabbitMq": {
      "HostName": "localhost",
      "Port": 5672,
      "Username": "guest",
      "Password": "guest",
      "VirtualHost": "/"
    },
    "Events": {
      "DomainEventTopic": "Bookstore.Domain.{EventName}",
      "IntegrationEventTopic": "Bookstore.Interchange.{EventName}",
      "ListenerGroup": "Default"
    }
  }
}
```

---

## Documentation

[https://docs.openddd.net](https://docs.openddd.net)

---

## Sample Project

Check out the [Bookstore Sample](https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore) for a complete application example.

---

## Contributing

Open to contributions from developers who care about DDD, clean architecture, and developer experience.

See [CONTRIBUTING.md](CONTRIBUTING.md) to get started.

---

## License

OpenDDD.NET is licensed under the [GPLv3 License](https://www.gnu.org/licenses/gpl-3.0.html). Feel free to use it in your own projects.
