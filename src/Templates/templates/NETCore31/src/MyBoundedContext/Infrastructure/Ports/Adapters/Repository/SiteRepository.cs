using OpenDDD.Infrastructure.Ports.Adapters.Repository;
using OpenDDD.NET.Services.DatabaseConnection;
using MyBoundedContext.Domain.Model.Site;

namespace MyBoundedContext.Infrastructure.Ports.Adapters.Repository
{
    public class SiteRepository : BaseRepository<Domain.Model.Site.Site, SiteId, IActionDatabaseConnection>, ISiteRepository
    {
        public SiteRepository(IActionDatabaseConnection databaseConnection) : base(databaseConnection)
        {
            
        }

        
    }
}
