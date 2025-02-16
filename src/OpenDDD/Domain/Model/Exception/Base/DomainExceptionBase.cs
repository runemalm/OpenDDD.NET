namespace OpenDDD.Domain.Model.Exception.Base
{
    public abstract class DomainExceptionBase : System.Exception
    {
        protected DomainExceptionBase(string message) : base(message) { }
    }
}
