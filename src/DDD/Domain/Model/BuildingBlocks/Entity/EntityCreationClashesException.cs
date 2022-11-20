using System;
using System.Collections.Generic;
using DDD.Domain.Model.BuildingBlocks.Aggregate;

namespace DDD.Domain.Model.BuildingBlocks.Entity
{
	public class EntityCreationClashesException<T> : DomainException where T : IAggregate
	{
        private readonly IEnumerable<string> ClashingProperties;

        public EntityCreationClashesException(string clashingProperty)
            : base($"Couldn't create the {typeof(T).Name.ToLower()} because a {typeof(T).Name.ToLower()} with that {clashingProperty.ToLower()} already exists.")
        {
            ClashingProperties = new List<string>() { clashingProperty };
        }

        public EntityCreationClashesException(IEnumerable<string> clashingProperties)
            : base($"Couldn't create the {typeof(T).Name.ToLower()} because a {typeof(T).Name.ToLower()} with the same {string.Join(", ", clashingProperties)} already exists.")
        {
            ClashingProperties = clashingProperties;
        }

        public EntityCreationClashesException(IEnumerable<string> clashingProperties, string message)
            : base(message)
        {
            ClashingProperties = clashingProperties;
        }

        public EntityCreationClashesException(IEnumerable<string> clashingProperties, string message, Exception inner)
            : base(message, inner)
        {
            ClashingProperties = clashingProperties;
        }
	}
}
