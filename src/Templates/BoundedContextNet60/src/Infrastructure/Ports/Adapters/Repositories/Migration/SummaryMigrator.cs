using DDD.Infrastructure.Ports.Adapters.Repository;
using Domain.Model.Summary;
using WeatherDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Infrastructure.Ports.Adapters.Repositories.Migration
{
	public class SummaryMigrator : Migrator<Summary>
	{
		public SummaryMigrator() : base(WeatherDomainModelVersion.Latest())
		{
			
		}
	}
}
