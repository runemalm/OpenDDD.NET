using System.Collections.Generic;

namespace DDD.Domain
{
	public interface IMigrator<T> where T : IBuildingBlock
	{
		T Migrate(T buildingBlock);

		IEnumerable<T> Migrate(IEnumerable<T> buildingBlocks);
	}
}
