.. note::

    OpenDDD.NET is currently in beta. Features and documentation are under active development and subject to change.

OpenDDD.NET
===========

OpenDDD.NET is an open-source framework for domain-driven design (DDD) development using C# and ASP.NET Core. It provides a set of powerful tools and abstractions to help developers build scalable, maintainable, and testable applications following the principles of DDD.

Purpose
-------

The purpose of OpenDDD.NET is to simplify the adoption of DDD principles by offering ready-to-use building blocks, enabling developers to focus on business logic rather than infrastructure concerns.

Key Features
------------

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
