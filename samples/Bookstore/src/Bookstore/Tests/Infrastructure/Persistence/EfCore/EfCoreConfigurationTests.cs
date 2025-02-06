using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.API.Options;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Repository.EfCore;
using Bookstore.Domain.Model;
using Bookstore.Infrastructure.Persistence.EfCore;

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

            // Register dependencies
            services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
            services.AddScoped(typeof(IRepository<Order, Guid>), typeof(EfCoreRepository<Order, Guid>));

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
            order.AddLineItem(Guid.NewGuid(), 19.99f);
            order.AddLineItem(Guid.NewGuid(), 29.99f);

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
