###############
Advanced Topics
###############

This section covers advanced topics for working with OpenDDD.NET. These concepts are useful for contributors or developers looking to extend the framework's functionality.

#############################
Configuring Service Lifetimes
#############################

By default, OpenDDD.NET auto-registers services (e.g., Actions, Domain Services, Repositories) with a **Transient** lifetime. If you need to specify a different lifetime (e.g., `Scoped` or `Singleton`), you can use the ``LifetimeAttribute``.

The ``LifetimeAttribute`` allows you to annotate your implementation classes with the desired lifetime, ensuring consistent behavior during auto-registration.

**Applying `LifetimeAttribute`:**

Use the `LifetimeAttribute` to customize the lifetime of a class:

.. code-block:: csharp

    using Microsoft.Extensions.DependencyInjection;

    [Lifetime(ServiceLifetime.Singleton)]
    public class InMemoryCustomerRepository : InMemoryRepositoryBase<Customer, Guid>, ICustomerRepository
    {
        public InMemoryCustomerRepository(ILogger<InMemoryCustomerRepository> logger) : base(logger)
        {
        }

        public Customer GetByEmail(string email, CancellationToken ct = default)
        {
            var customer = FindWithAsync(c => c.Email == email, ct).Result.FirstOrDefault();

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with email {email} was not found.");
            }

            return customer;
        }
    }

**Available Lifetimes:**

The ``LifetimeAttribute`` supports the following lifetimes:

- **Transient** (default): A new instance is created each time the service is resolved.
- **Scoped**: A new instance is created per scope (e.g., per HTTP request in ASP.NET Core).
- **Singleton**: A single instance is created and shared across the application's lifetime.

**When to Use `LifetimeAttribute`:**

- Use **Transient** for stateless operations and lightweight dependencies.
- Use **Scoped** for dependencies that need to share state within the same HTTP request.
- Use **Singleton** for expensive or shared resources that should be reused across the application.
