using OpenDDD.Domain.Model.Base;

namespace OpenDDD.Tests.Domain.Model
{
    public class TestEntity : EntityBase<Guid>
    {
        public string Description { get; private set; }

        private TestEntity() { }  // Needed if using EF Core..

        private TestEntity(Guid id, string description) : base(id)
        {
            Description = description;
        }

        public static TestEntity Create(string description)
        {
            return new TestEntity(Guid.NewGuid(), description);
        }
    }
}
