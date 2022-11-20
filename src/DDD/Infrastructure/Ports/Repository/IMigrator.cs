using System.Collections.Generic;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks;

namespace DDD.Infrastructure.Ports.Repository
{
	public interface IMigrator<T> where T : IBuildingBlock
	{
		T Migrate(T buildingBlock);

		IEnumerable<T> Migrate(IEnumerable<T> buildingBlocks);
	}
}
