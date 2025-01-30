.. note::

    OpenDDD.NET is currently in alpha. Features and documentation are under active development and subject to change.

###############
Version History
###############

**3.0.0-alpha.3 (2025-01-29)**

- **Domain Events**: Added support for domain events to enable communication between domain objects while maintaining encapsulation.
- **Integration Events**: Added support for integration events to facilitate communication between bounded contexts.
- **Event Listeners**: Added support for event listeners to handle domain and integration events with actions.
- **Transactional Outbox**: Added reliable event publishing by storing events in an outbox before processing and publishing to the message bus, ensuring consistency with database transactions.
- **Messaging Providers**: Added pluggable support for messaging backends, including in-memory and Azure Service Bus.
- **Infrastructure Services**: Added `IInfrastructureService` interface for managing infrastructure services, with automatic registration.
- **Repository Refactoring**: Refactored repository pattern by introducing `EfCoreRepository<TAggregate, TId>` as the default for the EfCore persistence provider, improving consistency and customization.
- **Configuration Refactoring**: Restructured OpenDDD.NET configuration system into hierarchical options classes, improving clarity and maintainability.

`View release on GitHub <https://github.com/runemalm/OpenDDD.NET/releases/tag/v3.0.0-alpha.3>`_

**3.0.0-alpha.2 (2025-01-21)**

- **Domain Services:** Added the `IDomainService` interface to mark domain services, which are now auto-registered.
- **Repositories:** Introduced support for repositories with `InMemoryRepositoryBase` as the base class for custom implementations and thus first supported persistence provider.
- **Actions:** Added the `IAction<TCommand, TReturns>` interface to mark actions, which are now auto-registered.
- **Sample Project:** Introduced the `Bookstore` sample project to demonstrate OpenDDD.NET usage.
- **Documentation Updates:** Expanded documentation with examples and guides for repositories, domain services, actions and the sample project.
- **Community:** Added an advanced topics section to the documentation, covering concepts useful for contributors or developers looking to extend the framework’s functionality.

`View release on GitHub <https://github.com/runemalm/OpenDDD.NET/releases/tag/v3.0.0-alpha.2>`_

**3.0.0-alpha.1 (2025-01-19)**

- **Major Version 3**: Initial alpha release with foundational features.

`View release on GitHub <https://github.com/runemalm/OpenDDD.NET/releases/tag/v3.0.0-alpha.1>`_
