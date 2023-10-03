using OpenDDD.Infrastructure.Ports.Repository;
using OpenDDD.NET.Services.DatabaseConnection;

namespace MyBoundedContext.Domain.Model.Property
{
    public interface IPropertyRepository : IRepository<Property, PropertyId, IActionDatabaseConnection>
    {
		
    }
}
