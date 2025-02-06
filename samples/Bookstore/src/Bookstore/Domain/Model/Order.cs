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

        public static Order Create(Guid customerId)
        {
            return new Order(Guid.NewGuid(), customerId);
        }

        public void AddLineItem(Guid bookId, float price)
        {
            var lineItem = LineItem.Create(bookId, price);
            LineItems.Add(lineItem);
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
