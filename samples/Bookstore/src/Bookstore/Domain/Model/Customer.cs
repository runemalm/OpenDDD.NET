using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Customer : AggregateRootBase<Guid>
    {
        public string Name;
        public string Email;

        private Customer(Guid id) : base(id) { }
    }
}
