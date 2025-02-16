using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.API.Options;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Repository.EfCore;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.EfCore.DatabaseSession;
using Bookstore.Domain.Model;
using Bookstore.Infrastructure.Persistence.EfCore;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Infrastructure.TransactionalOutbox;
using OpenDDD.Infrastructure.TransactionalOutbox.EfCore;

namespace Bookstore.Tests.Infrastructure.Persistence.EfCore
{
    public class EfCoreConfigurationTests
    {
        private readonly IServiceProvider _serviceProvider;

        public EfCoreConfigurationTests()
        {
            var services = new ServiceCollection();
            
            // Register logging
            services.AddLogging();

            // Manually configure OpenDDD options
            var options = new OpenDddOptions();
            services.AddSingleton(Options.Create(options));
            services.AddSingleton(options);

            // Add an in-memory database
            services.AddDbContext<BookstoreDbContext>(opts =>
                opts.UseInMemoryDatabase("TestDatabase"));
            services.AddScoped<OpenDddDbContextBase>(sp => sp.GetRequiredService<BookstoreDbContext>());

            // Register EfCoreDatabaseSession as the IDatabaseSession
            services.AddScoped<EfCoreDatabaseSession>();
            services.AddScoped<IDatabaseSession>(sp => sp.GetRequiredService<EfCoreDatabaseSession>());
            
            // Register dependencies
            services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
            services.AddScoped(typeof(IRepository<Order, Guid>), typeof(EfCoreRepository<Order, Guid>));
            
            // Register publishers
            services.AddScoped<IDomainPublisher, DomainPublisher>();
            services.AddScoped<IIntegrationPublisher, IntegrationPublisher>();
            
            // Register IOutboxRepository (EF Core implementation)
            services.AddScoped<IOutboxRepository, EfCoreOutboxRepository>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task CreateAndRetrieveOrder_WithLineItems_ShouldPersistCorrectly()
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Order, Guid>>();

            var ct = CancellationToken.None;

            // Arrange - Create and save an order with line items
            var order = Order.Create(Guid.NewGuid());
            order.AddLineItem(Guid.NewGuid(), Money.USD(19.99m));
            order.AddLineItem(Guid.NewGuid(), Money.USD(29.99m));

            await repository.SaveAsync(order, ct);

            // Act - Retrieve order
            var retrievedOrder = await repository.GetAsync(order.Id, ct);

            // Assert - Order and line items should be persisted
            Assert.NotNull(retrievedOrder);
            Assert.Equal(order.Id, retrievedOrder.Id);
            Assert.NotEmpty(retrievedOrder.LineItems);
            Assert.Equal(2, retrievedOrder.LineItems.Count);
        }
    }
}
