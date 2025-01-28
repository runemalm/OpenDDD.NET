using OpenDDD.Domain.Model;

namespace Bookstore.Domain.Model.Events.Domain
{
    public class CustomerRegistered : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredAt { get; set; }
        
        public CustomerRegistered() { }

        public CustomerRegistered(Guid customerId, string name, string email, DateTime registeredAt)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
            RegisteredAt = registeredAt;
        }

        public override string ToString()
        {
            return $"CustomerRegistered: CustomerId={CustomerId}, Name={Name}, Email={Email}, RegisteredAt={RegisteredAt}";
        }
    }
}
