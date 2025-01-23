using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Order : AggregateRootBase<Guid>
    {
        public Guid CustomerId;
        public IEnumerable<Item> Items;

        public Order(Guid id, Guid customerId, IEnumerable<Item> items) : base(id)
        {
            CustomerId = customerId;
            Items = items;
        }
    }
}

