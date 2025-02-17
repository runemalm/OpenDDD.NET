namespace OpenDDD.Domain.Model.Base
{
    public abstract class EntityBase<TId> : IEntity<TId>
    {
        public TId Id { get; protected set; } = default!;

        protected EntityBase() { }  // Needed if using EF Core..

        protected EntityBase(TId id)
        {
            Id = id;
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
