using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Order : AggregateRootBase<Guid>
    {
        public Guid CustomerId { get; private set; }
        public ICollection<LineItem> LineItems { get; private set; }
        
        private Order() { }  // Needed if using EF Core..

        private Order(Guid id, Guid customerId) : base(id)
        {
            CustomerId = customerId;
            LineItems = new List<LineItem>();
        }

        public static Order Create(Guid customerId)
        {
            return new Order(Guid.NewGuid(), customerId);
        }

        public void AddLineItem(Guid bookId, Money price)
        {
            var lineItem = LineItem.Create(bookId, price);
            LineItems.Add(lineItem);
        }
    }
}
