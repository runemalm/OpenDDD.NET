using OpenDDD.Infrastructure.Ports.Repository;
using OpenDDD.NET.Services.DatabaseConnection;

namespace MyBoundedContext.Domain.Model.Site
{
	public interface ISiteRepository : IRepository<Site, SiteId, IActionDatabaseConnection>
	{
		
	}
}
