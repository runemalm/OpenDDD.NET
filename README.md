
## DDD.NETCore

This is a framework for doing DDD with .Net Core. 

Star and/or follow the project to receive notifications when version 1.0.0 is released.

The aim is to provide a full framework to implement bounded contexts with .Net Core.

### Design: 

The design is based on these patterns:

- DDD
- Hexagonal Architecture
- Near-infinite Scalability ("Entity" concept)
- xUnit

### Theory: 
  
The following sources are recommended:
  
- [Domain-Driven Design: Tackling Complexity in the Heart of Software - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)  
- [Implementing Domain-Driven Design - Vaughn Vernon](https://www.amazon.com/Implementing-Domain-Driven-Design-Vaughn-Vernon/dp/0321834577)  
- [Hexagonal Architecture - Alistair Cockburn](https://alistair.cockburn.us/hexagonal-architecture/)
- [Life Beyond Distributed Transactions - Pat Helland](https://queue.acm.org/detail.cfm?id=3025012)
- [xUnit - Wikipedia](https://en.wikipedia.org/wiki/XUnit)

### Supported versions:

- .Net Core 7.0 (not tested)
- .Net Core 6.0
- .Net Core 5.0 (not tested)
- .Net Core 3.1
  
### Installation:

    Install-Package DDD.NETCore
  
### Example:

These files below are from the [shipping domain]() example project.

```c#
// Program.cs

TODO
```

```c#
// Startup.cs

TODO
```

```c#
// Shipment.cs

TODO
```
  
### Documentation:
  
Documentation is coming..

### Contribution:
  
If you want to contribute to the code base, create a pull request on the develop branch.

### Roadmap v1.0.0:

- [ ] GitHub README
- [ ] NuGet README
- [ ] Visual Studio Project Template .NET 7.0
- [x] Visual Studio Project Template .NET 6.0
- [ ] Visual Studio Project Template .NET 5.0
- [ ] Visual Studio Project Template .NET 3.1
- [x] Start Context
- [x] Stop Context
- [x] Control
- [x] On-the-fly aggregate migration
- [x] Auto-code Generation from domain.yml File
- [x] Postgres Dead Letter Queue
- [x] Memory Dead Letter Queue
- [x] Dead Letter Queue
- [x] Handle Poisonous Messages
- [x] Re-publish Failed Events
- [x] Postgres Outbox
- [x] Memory Outbox
- [x] Outbox
- [x] Domain Event Publishing
- [x] Integration Event Publishing
- [x] Rabbit Event Adapter
- [x] Memory Event Adapter
- [x] PubSub
- [x] Auth Domain Service
- [x] Auth
- [x] Aggregate
- [x] Entity
- [x] Value Object
- [x] Domain Event
- [x] Integration Event
- [x] Repository
- [x] Building Blocks
- [x] Domain Service
- [x] Infrastructure Service
- [x] Application Service
- [x] Auto-Generated Swagger HTTP Adapter Documentation
- [x] HTTP Adapter
- [x] Email Adapter
- [x] Persistence Service
- [x] Postgres Repository
- [x] Memory Repository

### Roadmap Future:

- [ ] Full Sample Project
- [ ] Quickstart Guide
- [ ] Documentation
- [ ] Azure Monitoring Adapter.
- [ ] Monitoring
- [ ] Task: Migrate all aggregate roots
- [ ] Tasks
- [ ] All-aggregates Migration Task
- [ ] Periodic Jobs
- [ ] Interval Jobs
- [ ] One-off Jobs
- [ ] Jobs
- [ ] Merge old API versions into current swagger json files.
- [ ] CLI Operation: Create action/aggregate/event/...
- [ ] CLI Operation: Migrate
- [ ] CLI Operation: Create Migration
- [ ] CLI Operation: Increment Domain Model Version
- [ ] Test Framework
- [ ] Tests
- [ ] Admin Dashboard
- [ ] Admin Tool: Inspect Dead Event
- [ ] Admin Tool: Republish Dead Event
- [ ] Administration

### Release Notes:

**1.0.0-alpha.1** - 2022-10-02
- New v1.0.0 alpha release.

**0.9.0-alpha7** - 2022-07-31
- First alpha test release on nuget.org.
