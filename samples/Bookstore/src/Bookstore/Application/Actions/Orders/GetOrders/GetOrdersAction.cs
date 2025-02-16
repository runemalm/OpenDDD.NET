using OpenDDD.Application;
using OpenDDD.Domain.Model;
using Bookstore.Domain.Model;

namespace Bookstore.Application.Actions.Orders.GetOrders
{
    public class GetOrdersAction : IAction<GetOrdersCommand, IEnumerable<Order>>
    {
        private readonly IRepository<Order, Guid> _orderRepository;

        public GetOrdersAction(IRepository<Order, Guid> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Order>> ExecuteAsync(GetOrdersCommand command, CancellationToken ct)
        {
            return await _orderRepository.FindAllAsync(ct);
        }
    }
}
