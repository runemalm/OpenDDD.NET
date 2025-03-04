using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Moq;
using Npgsql;
using OpenDDD.API.Options;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Repository.EfCore;
using OpenDDD.Infrastructure.TransactionalOutbox;
using OpenDDD.Tests.Base;
using OpenDDD.Tests.Base.Domain.Model;
using OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.DbContext.Postgres;

namespace OpenDDD.Tests.Integration.Infrastructure.Repository.EfCore.Postgres
{
    [Collection("EfCoreTests")]
    public class PostgresEfCoreRepositoryTests : IntegrationTests, IAsyncLifetime
    {
        private readonly string _connectionString;
        private readonly EfCoreDatabaseSession _session;
        private readonly Mock<IDomainPublisher> _mockDomainPublisher;
        private readonly Mock<IIntegrationPublisher> _mockIntegrationPublisher;
        private readonly Mock<IOutboxRepository> _mockOutboxRepository;
        private readonly EfCoreUnitOfWork _unitOfWork;
        private readonly EfCoreRepository<TestAggregateRoot, Guid> _repository;
        private readonly PostgresTestDbContext _dbContext;
        private readonly NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction = null!;
        private readonly OpenDddOptions _openDddOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<OpenDddDbContextBase> _dbContextLogger;

        public PostgresEfCoreRepositoryTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, enableLogging: true)
        {
            _connectionString = Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION_STRING")
                                ?? "Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpassword";

            _connection = new NpgsqlConnection(_connectionString);

            var options = new DbContextOptionsBuilder<PostgresTestDbContext>()
                .UseNpgsql(_connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public"))
                .EnableSensitiveDataLogging()
                .Options;

            _openDddOptions = new OpenDddOptions
            {
                AutoRegister = { EfCoreConfigurations = true }
            };

            var services = new ServiceCollection()
                .AddLogging(builder => builder.AddSimpleConsole())
                .BuildServiceProvider();
            _dbContextLogger = services.GetRequiredService<ILogger<PostgresTestDbContext>>();
            
            _dbContext = new PostgresTestDbContext(options, _openDddOptions, _dbContextLogger);

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
            // Re-create database
            await _dbContext.Database.EnsureDeletedAsync();
            EnsureDatabaseExists();
            
            // Run migrations
            _dbContext.Database.SetCommandTimeout(120);
            await _dbContext.Database.MigrateAsync();
            
            // Initialize connection and transaction
            _connection.Open();
            _transaction = await _connection.BeginTransactionAsync();
            _dbContext.Database.UseTransaction(_transaction);
        }

        public async Task DisposeAsync()
        {
            await _transaction.RollbackAsync();
            await _connection.CloseAsync();
        }
        
        private void EnsureDatabaseExists()
        {
            using var adminConnection = new NpgsqlConnection($"{_connectionString};Database=postgres");
            adminConnection.Open();

            var databaseName = "testdb";
    
            using var command = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", adminConnection);
            var databaseExists = command.ExecuteScalar() != null;

            if (!databaseExists)
            {
                using var createCommand = new NpgsqlCommand($"CREATE DATABASE {databaseName}", adminConnection);
                createCommand.ExecuteNonQuery();
            }
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
