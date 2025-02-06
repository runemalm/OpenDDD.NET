using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Order : AggregateRootBase<Guid>
    {
        public Guid CustomerId { get; private set; }
        public ICollection<LineItem> LineItems { get; private set; }

        private Order(Guid id, Guid customerId) : base(id)
        {
            CustomerId = customerId;
            LineItems = new List<LineItem>();
            Validate();
        }

        public static Order Create(Guid customerId, ICollection<LineItem> lineItems)
        {
            var order = new Order(Guid.NewGuid(), customerId);
            order.LineItems = lineItems ?? new List<LineItem>();
            return order;
        }

        private void Validate()
        {
            if (Id == Guid.Empty)
                throw new ArgumentException("Order ID cannot be empty.", nameof(Id));

            if (CustomerId == Guid.Empty)
                throw new ArgumentException("Customer ID cannot be empty.", nameof(CustomerId));
        }
    }
}
