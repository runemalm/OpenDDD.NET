.. note::

    OpenDDD.NET is currently in beta. Features and documentation are under active development and subject to change.

OpenDDD.NET
===========

**OpenDDD.NET** is an opinionated framework for building Domain-Driven Design (DDD) applications on ASP.NET Core.

It lets you focus on modeling your aggregates, domain services, and domain events — while the framework handles persistence, messaging, and transactional consistency.

> **Focus on your domain model. Forget the plumbing.**

---

Purpose
-------

OpenDDD.NET exists to help developers adopt DDD in real-world .NET applications without spending time wiring infrastructure.

It provides all the essential building blocks — aggregates, repositories, domain services, event handling, persistence, and messaging — with drop-in setup, automatic registration, and a convention-over-configuration approach.

---

Key Features
------------

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

Source Code
-----------

The source code is available on GitHub:  
`https://github.com/runemalm/OpenDDD.NET <https://github.com/runemalm/OpenDDD.NET>`_

---

.. userguide-docs:
.. toctree::
  :maxdepth: 1
  :caption: User Guide

  userguide

.. building-blocks-docs:
.. toctree::
  :maxdepth: 1
  :caption: Building Blocks

  building-blocks

.. configuration-docs:
.. toctree::
  :maxdepth: 1
  :caption: Configuration

  configuration

.. releases-docs:
.. toctree::
  :maxdepth: 1
  :caption: Releases

  releases

You can find the source code for `OpenDDD.NET` in our `GitHub repository <https://github.com/runemalm/OpenDDD.NET>`_.
