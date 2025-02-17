namespace OpenDDD.Domain.Model.Base
{
    public abstract class AggregateRootBase<TId> : EntityBase<TId>, IAggregateRoot<TId>
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        protected AggregateRootBase() : base() { }  // Needed if using EF Core..

        protected AggregateRootBase(TId id) : base(id)
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }
    }
}
