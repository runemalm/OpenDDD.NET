using Microsoft.AspNetCore.Mvc;
using Bookstore.Application.Actions.GetCustomer;
using Bookstore.Application.Actions.GetCustomers;
using Bookstore.Application.Actions.RegisterCustomer;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Adapters.WebAPI.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly RegisterCustomerAction _registerCustomerAction;
        private readonly GetCustomerAction _getCustomerAction;
        private readonly GetCustomersAction _getCustomersAction;

        public CustomerController(
            RegisterCustomerAction registerCustomerAction,
            GetCustomerAction getCustomerAction,
            GetCustomersAction getCustomersAction)
        {
            _registerCustomerAction = registerCustomerAction;
            _getCustomerAction = getCustomerAction;
            _getCustomersAction = getCustomersAction;
        }

        [HttpPost("register-customer")]
        public async Task<ActionResult<Customer>> RegisterCustomer([FromBody] RegisterCustomerCommand command, CancellationToken ct)
        {
            try
            {
                var customer = await _registerCustomerAction.ExecuteAsync(command, ct);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("get-customer")]
        public async Task<ActionResult<Customer>> GetCustomer([FromQuery] Guid id, CancellationToken ct)
        {
            try
            {
                var command = new GetCustomerCommand(id);
                var customer = await _getCustomerAction.ExecuteAsync(command, ct);
                return Ok(customer);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Customer with ID {id} not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        
        [HttpGet("get-customers")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers(CancellationToken ct)
        {
            try
            {
                var command = new GetCustomersCommand();
                var customers = await _getCustomersAction.ExecuteAsync(command, ct);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
