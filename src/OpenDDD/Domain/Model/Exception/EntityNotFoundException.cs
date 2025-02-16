using OpenDDD.Domain.Model.Exception.Base;

namespace OpenDDD.Domain.Model.Exception
{
    public class EntityNotFoundException : DomainExceptionBase
    {
        public EntityNotFoundException(string entityName, Guid entityId)
            : base($"{entityName} with ID {entityId} was not found.") { }
    }
}
