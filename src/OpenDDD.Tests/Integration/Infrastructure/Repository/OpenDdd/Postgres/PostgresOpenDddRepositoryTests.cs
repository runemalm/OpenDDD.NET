using System.Linq.Expressions;
using FluentAssertions;
using Npgsql;
using Xunit.Abstractions;
using OpenDDD.API.Extensions;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers;
using OpenDDD.Infrastructure.Persistence.Serializers;
using OpenDDD.Infrastructure.Repository.OpenDdd.Postgres;
using OpenDDD.Tests.Base;
using OpenDDD.Tests.Domain.Model;

namespace OpenDDD.Tests.Integration.Infrastructure.Repository.OpenDdd.Postgres
{
    [Collection("PostgresTests")]
    public class PostgresOpenDddRepositoryTests : IntegrationTests, IAsyncLifetime
    {
        private readonly string _connectionString;
        private readonly PostgresDatabaseSession _session;
        private readonly IAggregateSerializer _serializer;
        private readonly PostgresOpenDddRepository<TestAggregateRoot, Guid> _repository;
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;

        public PostgresOpenDddRepositoryTests(ITestOutputHelper testOutputHelper) 
            : base(testOutputHelper, enableLogging: true)
        {
            _connectionString = Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION_STRING") 
                                ?? "Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpassword";

            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();

            _session = new PostgresDatabaseSession(_connection);
            _serializer = new OpenDddAggregateSerializer();
            _repository = new PostgresOpenDddRepository<TestAggregateRoot, Guid>(_session, _serializer);
        }

        public async Task InitializeAsync()
        {
            var tableName = typeof(TestAggregateRoot).Name.ToLower().Pluralize();

            var createTableQuery = $@"
                        CREATE TABLE {tableName} (
                            id UUID PRIMARY KEY,
                            data JSONB NOT NULL
                        );";

            await using var createCmd = new NpgsqlCommand(createTableQuery, _connection, _transaction);
            await createCmd.ExecuteNonQueryAsync();
        }

        public async Task DisposeAsync()
        {
            await _transaction.RollbackAsync();
            await _connection.CloseAsync();
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
