using Xunit;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers;
using OpenDDD.Tests.Domain.Model;

namespace OpenDDD.Tests.Infrastructure.Persistence.OpenDdd.Serializers
{
    public class OpenDddAggregateSerializerTests
    {
        private readonly OpenDddAggregateSerializer _serializer;

        public OpenDddAggregateSerializerTests()
        {
            _serializer = new OpenDddAggregateSerializer();
        }

        [Fact]
        public void Should_Serialize_AggregateRoot_To_Json()
        {
            var entity1 = TestEntity.Create("First Entity");
            var entity2 = TestEntity.Create("Second Entity");
            var valueObject = new TestValueObject(100, "Test Value");
            var aggregate = TestAggregateRoot.Create("Aggregate Root", new List<TestEntity> { entity1, entity2 }, valueObject);

            var json = _serializer.Serialize(aggregate);

            Assert.NotNull(json);
            Assert.Contains("\"name\":\"Aggregate Root\"", json);
            Assert.Contains("\"entities\"", json);
            Assert.Contains("\"number\":100", json);
            Assert.Contains("\"text\":\"Test Value\"", json);
        }

        [Fact]
        public void Should_Deserialize_Json_Back_To_AggregateRoot()
        {
            var json = "{\"id\":\"550e8400-e29b-41d4-a716-446655440000\",\"name\":\"Deserialized Aggregate\",\"entities\":[{\"id\":\"8a1f5e65-d5a4-48c1-9f17-c640ef7d5432\",\"description\":\"Entity One\"}],\"value\":{\"number\":42,\"text\":\"Deserialized Value\"}}";

            var aggregate = _serializer.Deserialize<TestAggregateRoot>(json);

            Assert.NotNull(aggregate);
            Assert.Equal(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), aggregate.Id);
            Assert.Equal("Deserialized Aggregate", aggregate.Name);
            Assert.Single(aggregate.Entities);
            Assert.Equal("Entity One", aggregate.Entities[0].Description);
            Assert.Equal(42, aggregate.Value.Number);
            Assert.Equal("Deserialized Value", aggregate.Value.Text);
        }

        [Fact]
        public void Should_Preserve_ValueObject_During_Serialization_And_Deserialization()
        {
            var aggregate = TestAggregateRoot.Create("Aggregate with Value Object", new List<TestEntity>(), new TestValueObject(200, "Persisted Value"));

            var json = _serializer.Serialize(aggregate);
            var deserializedAggregate = _serializer.Deserialize<TestAggregateRoot>(json);

            Assert.NotNull(deserializedAggregate);
            Assert.Equal(200, deserializedAggregate.Value.Number);
            Assert.Equal("Persisted Value", deserializedAggregate.Value.Text);
        }
    }
}
