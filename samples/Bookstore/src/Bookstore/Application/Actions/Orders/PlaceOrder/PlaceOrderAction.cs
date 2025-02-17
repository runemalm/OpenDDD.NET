using OpenDDD.Application;
using OpenDDD.Domain.Model;
using Bookstore.Domain.Model;
using Bookstore.Domain.Service;

namespace Bookstore.Application.Actions.Orders.PlaceOrder
{
    public class PlaceOrderAction : IAction<PlaceOrderCommand, Order>
    {
        private readonly IRepository<Order, Guid> _orderRepository;
        private readonly IOrderDomainService _orderDomainService;

        public PlaceOrderAction(
            IRepository<Order, Guid> orderRepository,
            IOrderDomainService orderDomainService)
        {
            _orderRepository = orderRepository;
            _orderDomainService = orderDomainService;
        }

        public async Task<Order> ExecuteAsync(PlaceOrderCommand command, CancellationToken ct)
        {
            var order = await _orderDomainService.PlaceOrderAsync(command.CustomerId, command.BookId, ct);
            await _orderRepository.SaveAsync(order, ct);
            return order;
        }
    }
}
