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
            Validate();
        }

        public static Customer Create(string name, string email)
        {
            return new Customer(Guid.NewGuid(), name, email);
        }

        public void ChangeName(string name)
        {
            Name = name;
            Validate();
        }

        private void Validate()
        {
            if (Id == Guid.Empty)
                throw new ArgumentException("Customer ID cannot be empty.", nameof(Id));

            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Customer name cannot be empty.", nameof(Name));

            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
                throw new ArgumentException("Invalid email address.", nameof(Email));
        }
    }
}