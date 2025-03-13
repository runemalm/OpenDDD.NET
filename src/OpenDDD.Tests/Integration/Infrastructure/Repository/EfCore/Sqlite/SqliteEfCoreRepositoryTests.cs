using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Moq;
using OpenDDD.API.Options;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Repository.EfCore;
using OpenDDD.Infrastructure.TransactionalOutbox;
using OpenDDD.Tests.Base;
using OpenDDD.Tests.Base.Domain.Model;
using OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.DbContext.Sqlite;

namespace OpenDDD.Tests.Integration.Infrastructure.Repository.EfCore.Sqlite
{
    [Collection("EfCoreTests")]
    public class SqliteEfCoreRepositoryTests : IntegrationTests, IAsyncLifetime
    {
        private readonly EfCoreDatabaseSession _session;
        private readonly Mock<IDomainPublisher> _mockDomainPublisher;
        private readonly Mock<IIntegrationPublisher> _mockIntegrationPublisher;
        private readonly Mock<IOutboxRepository> _mockOutboxRepository;
        private readonly EfCoreUnitOfWork _unitOfWork;
        private readonly EfCoreRepository<TestAggregateRoot, Guid> _repository;
        private readonly SqliteTestDbContext _dbContext;
        private readonly OpenDddOptions _openDddOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<OpenDddDbContextBase> _dbContextLogger;
        private readonly string _connectionString = "DataSource=:memory:";

        public SqliteEfCoreRepositoryTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, enableLogging: true)
        {
            var options = new DbContextOptionsBuilder<SqliteTestDbContext>()
                .UseSqlite(_connectionString)
                .EnableSensitiveDataLogging()
                .Options;

            _openDddOptions = new OpenDddOptions
            {
                AutoRegister = { EfCoreConfigurations = true }
            };

            var services = new ServiceCollection()
                .AddLogging(builder => builder.AddSimpleConsole())
                .BuildServiceProvider();
            _dbContextLogger = services.GetRequiredService<ILogger<SqliteTestDbContext>>();

            _dbContext = new SqliteTestDbContext(options, _openDddOptions, _dbContextLogger);

            _session = new EfCoreDatabaseSession(_dbContext);

            _mockDomainPublisher = new Mock<IDomainPublisher>();
            _mockIntegrationPublisher = new Mock<IIntegrationPublisher>();
            _mockOutboxRepository = new Mock<IOutboxRepository>();

            _loggerFactory = services.GetRequiredService<ILoggerFactory>();

            _unitOfWork = new EfCoreUnitOfWork(
                _session, 
                _mockDomainPublisher.Object, 
                _mockIntegrationPublisher.Object, 
                _mockOutboxRepository.Object, 
                _loggerFactory.CreateLogger<EfCoreUnitOfWork>()
            );
            _repository = new EfCoreRepository<TestAggregateRoot, Guid>(_unitOfWork);
        }

        public async Task InitializeAsync()
        {
            await _dbContext.Database.OpenConnectionAsync();
            await _dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }

        [Fact]
        public async Task SaveAsync_ShouldInsertOrUpdateEntity()
        {
            // Arrange
            var aggregate = TestAggregateRoot.Create(
                "Initial Root",
                new List<TestEntity> { TestEntity.Create("Entity 1"), TestEntity.Create("Entity 2") },
                new TestValueObject(100, "Value Object Data")
            );

            // Act
            await _repository.SaveAsync(aggregate, CancellationToken.None);
            var retrieved = await _repository.GetAsync(aggregate.Id, CancellationToken.None);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved.Id.Should().Be(aggregate.Id);
            retrieved.Name.Should().Be("Initial Root");
            retrieved.Entities.Should().HaveCount(2);
            retrieved.Value.Number.Should().Be(100);

            // Act (update)
            aggregate = TestAggregateRoot.Create(
                "Updated Root",
                new List<TestEntity> { TestEntity.Create("Updated Entity") },
                new TestValueObject(200, "Updated Value")
            );

            await _repository.SaveAsync(aggregate, CancellationToken.None);
            var updated = await _repository.GetAsync(aggregate.Id, CancellationToken.None);

            // Assert (update)
            updated.Name.Should().Be("Updated Root");
            updated.Entities.Should().HaveCount(1);
            updated.Entities.First().Description.Should().Be("Updated Entity");
            updated.Value.Number.Should().Be(200);
        }

        [Fact]
        public async Task FindAsync_ShouldReturnEntityIfExists()
        {
            // Arrange
            var aggregate = TestAggregateRoot.Create("Find Test", new List<TestEntity>(), new TestValueObject(50, "Find Test Value"));
            await _repository.SaveAsync(aggregate, CancellationToken.None);

            // Act
            var result = await _repository.FindAsync(aggregate.Id, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(aggregate.Id);
        }

        [Fact]
        public async Task FindAsync_ShouldReturnNullIfNotExists()
        {
            // Act
            var result = await _repository.FindAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindWithAsync_ShouldReturnFilteredResults()
        {
            // Arrange
            var aggregate1 = TestAggregateRoot.Create("Filter Match", new List<TestEntity>(), new TestValueObject(300, "Match"));
            var aggregate2 = TestAggregateRoot.Create("No Match", new List<TestEntity>(), new TestValueObject(400, "Different"));
            await _repository.SaveAsync(aggregate1, CancellationToken.None);
            await _repository.SaveAsync(aggregate2, CancellationToken.None);

            // Act
            Expression<Func<TestAggregateRoot, bool>> filter = a => a.Value.Number == 300;
            var results = (await _repository.FindWithAsync(filter, CancellationToken.None)).ToList();

            // Assert
            results.Should().HaveCount(1);
            results[0].Name.Should().Be("Filter Match");
        }

        [Fact]
        public async Task FindAllAsync_ShouldReturnAllEntities()
        {
            // Arrange
            var aggregate1 = TestAggregateRoot.Create("Entity 1", new List<TestEntity>(), new TestValueObject(10, "VO 1"));
            var aggregate2 = TestAggregateRoot.Create("Entity 2", new List<TestEntity>(), new TestValueObject(20, "VO 2"));
            await _repository.SaveAsync(aggregate1, CancellationToken.None);
            await _repository.SaveAsync(aggregate2, CancellationToken.None);

            // Act
            var results = (await _repository.FindAllAsync(CancellationToken.None)).ToList();

            // Assert
            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntity()
        {
            // Arrange
            var aggregate = TestAggregateRoot.Create("To be deleted", new List<TestEntity>(), new TestValueObject(99, "Delete"));
            await _repository.SaveAsync(aggregate, CancellationToken.None);

            // Act
            await _repository.DeleteAsync(aggregate, CancellationToken.None);
            var result = await _repository.FindAsync(aggregate.Id, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
