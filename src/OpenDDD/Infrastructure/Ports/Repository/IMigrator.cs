using System.Collections.Generic;
using OpenDDD.Domain.Model.BuildingBlocks.Aggregate;

namespace OpenDDD.Infrastructure.Ports.Repository
{
	public interface IMigrator<T> where T : IAggregate
	{
		T Migrate(T buildingBlock);

		IEnumerable<T> Migrate(IEnumerable<T> buildingBlocks);
	}
}
