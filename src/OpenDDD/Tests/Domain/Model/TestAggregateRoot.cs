using OpenDDD.Domain.Model.Base;

namespace OpenDDD.Tests.Domain.Model
{
    public class TestAggregateRoot : AggregateRootBase<Guid>
    {
        public string Name { get; private set; }
        public List<TestEntity> Entities { get; private set; }
        public TestValueObject Value { get; private set; }

        private TestAggregateRoot() { }  // Needed if using EF Core..

        private TestAggregateRoot(Guid id, string name, List<TestEntity> entities, TestValueObject value) : base(id)
        {
            Name = name;
            Entities = entities ?? new List<TestEntity>();
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static TestAggregateRoot Create(string name, List<TestEntity> entities, TestValueObject value)
        {
            return new TestAggregateRoot(Guid.NewGuid(), name, entities, value);
        }
    }
}
