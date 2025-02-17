using OpenDDD.Domain.Model.Exception.Base;

namespace OpenDDD.Domain.Model.Exception
{
    public class DomainException : DomainExceptionBase
    {
        public DomainException(string message)
            : base(message) { }
    }
}
