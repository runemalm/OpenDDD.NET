using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Customer : AggregateRootBase<Guid>
    {
        public string Name { get; private set; }
        public string Email { get; private set; }

        private Customer(Guid id, string name, string email) : base(id)
        {
            Name = name;
            Email = email;
        }

        public static Customer Create(string name, string email)
        {
            return new Customer(Guid.NewGuid(), name, email);
        }

        public void ChangeName(string name)
        {
            Name = name;
        }
    }
}
