.. note::

    OpenDDD.NET is currently in alpha. Features and documentation are under active development and subject to change.

OpenDDD.NET
===========

OpenDDD.NET is an open-source framework for domain-driven design (DDD) development using C# and .NET. It provides a set of powerful tools and abstractions to help developers build scalable, maintainable, and testable applications following the principles of DDD.

Purpose
-------

The purpose of OpenDDD.NET is to simplify the adoption of DDD principles by offering ready-to-use building blocks, enabling developers to focus on business logic rather than infrastructure concerns.

Key Features
------------

- **Aggregates**: Define domain aggregates with clear boundaries and encapsulate domain logic within them. (Implemented)
- **Entities and Value Objects**: Create entities and value objects to represent domain concepts and ensure strong type safety. (Implemented)
- **Repositories**: Abstract away data access and enable persistence of domain objects. (Planned)
- **Domain Events**: Facilitate communication between domain objects while maintaining loose coupling. (Planned)
- **Integration Events**: Enable communication between bounded contexts in distributed systems. (Planned)
- **Event Listeners**: Manage event listeners to handle domain and integration events for scalable, event-driven architectures. (Planned)
- **Domain Services**: Encapsulate domain-specific operations that do not naturally belong to an entity or value object. (Implemented)
- **Application Services**: Use Action classes to coordinate the execution of domain logic in response to commands. (Implemented)
- **Infrastructure Services**: Provide implementations for technical concerns such as logging, email, or external integrations. (Planned)
- **Transactional Outbox**: Ensure event consistency by persisting and publishing events as part of database transactions. (Planned)

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

.. advanced-topics-docs:
.. toctree::
  :maxdepth: 1
  :caption: Advanced Topics

  advanced-topics

.. releases-docs:
.. toctree::
  :maxdepth: 1
  :caption: Releases

  releases

You can find the source code for `OpenDDD.NET` in our `GitHub repository <https://github.com/runemalm/OpenDDD.NET>`_.
