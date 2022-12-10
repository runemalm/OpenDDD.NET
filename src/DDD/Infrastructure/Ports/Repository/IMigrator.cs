using System.Collections.Generic;
using DDD.Domain.Model.BuildingBlocks.Aggregate;

namespace DDD.Infrastructure.Ports.Repository
{
	public interface IMigrator<T> where T : IAggregate
	{
		T Migrate(T buildingBlock);

		IEnumerable<T> Migrate(IEnumerable<T> buildingBlocks);
	}
}
