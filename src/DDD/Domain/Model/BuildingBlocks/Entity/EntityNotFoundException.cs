using System;
using DDD.Domain.Model.BuildingBlocks.Aggregate;

namespace DDD.Domain.Model.BuildingBlocks.Entity
{
	public class EntityNotFoundException<T> : DomainException where T : IAggregate
	{
        private readonly EntityId EntityId;

        public EntityNotFoundException(EntityId entityId) : base($"Couldn't find {typeof(T).Name} with ID '{entityId.Value}'.")
        {
            EntityId = entityId;
        }

        public EntityNotFoundException(EntityId entityId, string message)
            : base(message)
        {
            EntityId = entityId;
        }

        public EntityNotFoundException(EntityId entityId, string message, Exception inner)
            : base(message, inner)
        {
            EntityId = entityId;
        }
	}
}
