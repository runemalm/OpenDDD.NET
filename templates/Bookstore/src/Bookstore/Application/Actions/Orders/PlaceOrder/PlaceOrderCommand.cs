using OpenDDD.Application;

namespace Bookstore.Application.Actions.Orders.PlaceOrder
{
    public class PlaceOrderCommand : ICommand
    {
        public Guid CustomerId { get; }
        public Guid BookId { get; }

        public PlaceOrderCommand(Guid customerId, Guid bookId)
        {
            CustomerId = customerId;
            BookId = bookId;
        }
    }
}
