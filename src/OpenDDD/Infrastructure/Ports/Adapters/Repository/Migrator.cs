using System.Collections.Generic;
using System.Linq;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.BuildingBlocks.Aggregate;
using OpenDDD.Infrastructure.Ports.Repository;

namespace OpenDDD.Infrastructure.Ports.Adapters.Repository
{
	public class Migrator<T> : IMigrator<T> where T : IAggregate 
	{
		private readonly DomainModelVersion _latestVersion;
		
		public Migrator(DomainModelVersion latestVersion)
		{
			_latestVersion = latestVersion;
		}

		public T Migrate(T entity)
		{
			DomainModelVersion at = entity.DomainModelVersion;

			while (at < _latestVersion)
			{
				var methodName = $"From_v{at.ToString().Replace('.', '_')}";
				var method = GetType().GetMethod(methodName, new [] {typeof(T)});
				if (method != null)
				{
					entity = (T)method.Invoke(this, new object[]{entity});
					at = entity.DomainModelVersion;
				}
				else
				{
					at = _latestVersion;
				}
			}

			return entity;
		}

		public IEnumerable<T> Migrate(IEnumerable<T> buildingBlocks)
			=> buildingBlocks.Select(Migrate);
	}
}
