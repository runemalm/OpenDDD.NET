using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Customer : AggregateRootBase<Guid>
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        
        private Customer() : base(Guid.Empty) { }

        public Customer(Guid id, string name, string email) : base(id)
        {
            Name = name;
            Email = email;
        }
    }
}
