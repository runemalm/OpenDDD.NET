.. note::

    OpenDDD.NET is currently in alpha. Features and documentation are under active development and subject to change.

##########
Aggregates
##########

Aggregates represent the core units of your domain model, defining boundaries within which domain logic and consistency are enforced. Aggregates have a single root entity known as the Aggregate Root.

.. code-block:: csharp

    public class Order : AggregateRootBase<Guid>
    {
        public string CustomerName { get; private set; }

        public Order(Guid id, string customerName) : base(id)
        {
            CustomerName = customerName;
        }
    }

########
Entities
########

Entities are domain objects with unique identities that remain consistent throughout their lifecycle. They often make up the components of an aggregate.

.. code-block:: csharp

    public class Product : EntityBase<Guid>
    {
        public string Name { get; private set; }

        public Product(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

#############
Value Objects
#############

Value Objects represent immutable domain concepts without identity. They are defined by their values and are interchangeable when their values are the same.

.. code-block:: csharp

    public class Money : IValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
    }

############
Repositories
############

Repositories abstract away persistence concerns, enabling interaction with aggregates and entities while keeping domain logic clean. This feature is currently under development.

#######
Actions
#######

Application Actions coordinate the execution of domain logic in response to commands. They are central to the application layer.

.. code-block:: csharp

    public class PlaceOrderAction : IAction<PlaceOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;

        public PlaceOrderAction(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Guid> ExecuteAsync(PlaceOrderCommand command, CancellationToken ct)
        {
            var order = new Order(Guid.NewGuid(), command.CustomerName);
            await _orderRepository.SaveAsync(order, ct);
            return order.Id;
        }
    }

#################
Domain Events
#################

Domain Events facilitate communication between domain objects while maintaining loose coupling. This feature is currently under development.

######################
Integration Events
######################

Integration Events enable communication between bounded contexts in distributed systems. This feature is currently under development.

###################
Event Listeners
###################

Event Listeners manage domain and integration events, supporting scalable, event-driven architectures. This feature is currently under development.

###################
Domain Services
###################

Domain Services encapsulate domain-specific operations that do not naturally belong to an entity or value object. This feature is currently under development.

#########################
Infrastructure Services
#########################

Infrastructure Services provide implementations for technical concerns such as logging, email, or external integrations. This feature is currently under development.

########################
Transactional Outbox
########################

The Transactional Outbox ensures event consistency by persisting and publishing events as part of database transactions. This feature is currently under development.

---

Explore these building blocks in your own projects to unlock the full potential of OpenDDD.NET and simplify the implementation of DDD principles.
