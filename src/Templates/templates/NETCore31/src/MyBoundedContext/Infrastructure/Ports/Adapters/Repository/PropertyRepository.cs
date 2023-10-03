using OpenDDD.Infrastructure.Ports.Adapters.Repository;
using OpenDDD.NET.Services.DatabaseConnection;
using MyBoundedContext.Domain.Model.Property;

namespace MyBoundedContext.Infrastructure.Ports.Adapters.Repository
{
    public class PropertyRepository : BaseRepository<Property, PropertyId, IActionDatabaseConnection>, IPropertyRepository
    {
        public PropertyRepository(IActionDatabaseConnection databaseConnection) : base(databaseConnection)
        {
            
        }

        
    }
}
