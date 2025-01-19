namespace OpenDDD.Domain.Model.Base
{
    public abstract class AggregateRootBase<TId> : EntityBase<TId>, IAggregateRoot<TId>
    {
        protected AggregateRootBase() : base() { }

        protected AggregateRootBase(TId id) : base(id) { }
    }
}
