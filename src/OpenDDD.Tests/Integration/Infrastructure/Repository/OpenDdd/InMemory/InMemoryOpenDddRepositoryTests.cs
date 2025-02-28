using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit.Abstractions;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers;
using OpenDDD.Infrastructure.Persistence.Serializers;
using OpenDDD.Infrastructure.Persistence.Storage.InMemory;
using OpenDDD.Infrastructure.Repository.OpenDdd.InMemory;
using OpenDDD.Tests.Base;
using OpenDDD.Tests.Domain.Model;

namespace OpenDDD.Tests.Integration.Infrastructure.Repository.OpenDdd.InMemory
{
    [Collection("InMemoryTests")]
    public class InMemoryOpenDddRepositoryTests : IntegrationTests, IAsyncLifetime
    {
        private readonly ILogger<InMemoryKeyValueStorage> _storageLogger;
        private readonly ILogger<InMemoryDatabaseSession> _sessionLogger;
        private readonly InMemoryKeyValueStorage _storage;
        private readonly InMemoryDatabaseSession _session;
        private readonly IAggregateSerializer _serializer;
        private readonly InMemoryOpenDddRepository<TestAggregateRoot, Guid> _repository;

        public InMemoryOpenDddRepositoryTests(ITestOutputHelper testOutputHelper) 
            : base(testOutputHelper, enableLogging: true)
        {
            _storageLogger = LoggerFactory.CreateLogger<InMemoryKeyValueStorage>();
            _sessionLogger = LoggerFactory.CreateLogger<InMemoryDatabaseSession>();
            _storage = new InMemoryKeyValueStorage(_storageLogger);
            _session = new InMemoryDatabaseSession(_storage, _sessionLogger);
            _serializer = new OpenDddAggregateSerializer();
            _repository = new InMemoryOpenDddRepository<TestAggregateRoot, Guid>(_session, _serializer);
        }

        public async Task InitializeAsync()
        {
            await _storage.ClearAsync(CancellationToken.None);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
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
