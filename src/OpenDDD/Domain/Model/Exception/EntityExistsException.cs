using OpenDDD.Domain.Model.Exception.Base;

namespace OpenDDD.Domain.Model.Exception
{
    public class EntityExistsException : DomainExceptionBase
    {
        public EntityExistsException(string entityName, string constraint)
            : base($"{entityName} already exists with {constraint}.") { }
    }
}
