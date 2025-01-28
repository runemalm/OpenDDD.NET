using OpenDDD.Domain.Model;

namespace Bookstore.Domain.Model.Events
{
    public class CustomerRegisteredDomainEvent : IDomainEvent
    {
        public Guid CustomerId { get; }
        public string Name { get; }
        public string Email { get; }

        public DateTime RegisteredAt { get; }

        public CustomerRegisteredDomainEvent(Guid customerId, string name, string email, DateTime registeredAt)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
            RegisteredAt = registeredAt;
        }

        public override string ToString()
        {
            return $"CustomerRegisteredDomainEvent: CustomerId={CustomerId}, Name={Name}, Email={Email}, RegisteredAt={RegisteredAt}";
        }
    }
}
