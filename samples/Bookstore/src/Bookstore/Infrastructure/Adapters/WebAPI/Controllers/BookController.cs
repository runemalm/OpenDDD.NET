using Microsoft.AspNetCore.Mvc;
using Bookstore.Application.Actions.SearchBooks;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Adapters.WebAPI.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly SearchBooksAction _searchBookAction;

        public BookController(SearchBooksAction searchBookAction)
        {
            _searchBookAction = searchBookAction;
        }

        [HttpGet("search-books")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks([FromQuery] string titleFreeText, CancellationToken ct)
        {
            try
            {
                var command = new SearchBooksCommand(titleFreeText);
                var books = await _searchBookAction.ExecuteAsync(command, ct);
                return Ok(books);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
