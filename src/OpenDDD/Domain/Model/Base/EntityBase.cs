namespace OpenDDD.Domain.Model.Base
{
    public abstract class EntityBase<TId> : IEntity<TId>
    {
        public TId Id { get; protected set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // EF Core requires a parameterless constructor
        protected EntityBase() { }

        protected EntityBase(TId id)
        {
            Id = id;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not EntityBase<TId> other)
                return false;

            return Id!.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id!.GetHashCode();
        }
    }
}
