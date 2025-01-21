using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Customer : AggregateRootBase<Guid>
    {
        public string Name;
        public string Email;

        public Customer(Guid id, string name, string email) : base(id)
        {
            Name = name;
            Email = email;
        }
    }
}
