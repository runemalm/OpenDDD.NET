using Microsoft.AspNetCore.Mvc;
using Bookstore.Domain.Model;
using Bookstore.Application.Actions.Orders.GetOrders;
using Bookstore.Application.Actions.Orders.PlaceOrder;

namespace Bookstore.Infrastructure.Adapters.WebAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly PlaceOrderAction _placeOrderAction;
        private readonly GetOrdersAction _getOrdersAction;

        public OrderController(
            PlaceOrderAction placeOrderAction,
            GetOrdersAction getOrdersAction)
        {
            _placeOrderAction = placeOrderAction;
            _getOrdersAction = getOrdersAction;
        }

        [HttpPost("place-order")]
        public async Task<ActionResult<Order>> PlaceOrder([FromBody] PlaceOrderCommand command, CancellationToken ct)
        {
            try
            {
                var order = await _placeOrderAction.ExecuteAsync(command, ct);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("get-orders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(CancellationToken ct)
        {
            try
            {
                var command = new GetOrdersCommand();
                var orders = await _getOrdersAction.ExecuteAsync(command, ct);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}