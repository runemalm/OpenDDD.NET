using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Order : AggregateRootBase<Guid>
    {
        public Guid CustomerId { get; private set; }
        public ICollection<LineItem> LineItems { get; private set; }

        private Order() : base(Guid.Empty)
        {
            LineItems = new List<LineItem>();
        }

        public Order(Guid id, Guid customerId, ICollection<LineItem> lineItems) : base(id)
        {
            CustomerId = customerId;
            LineItems = lineItems ?? new List<LineItem>();
        }
    }
}
