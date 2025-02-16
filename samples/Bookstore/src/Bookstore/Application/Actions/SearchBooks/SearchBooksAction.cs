using OpenDDD.Application;
using OpenDDD.Domain.Model;
using Bookstore.Domain.Model;

namespace Bookstore.Application.Actions.SearchBooks
{
    public class SearchBooksAction : IAction<SearchBooksCommand, IEnumerable<Book>>
    {
        private readonly IRepository<Book, Guid> _bookRepository;

        public SearchBooksAction(IRepository<Book, Guid> bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<Book>> ExecuteAsync(SearchBooksCommand command, CancellationToken ct)
        {
            var books = await _bookRepository.FindWithAsync(book => 
                book.Name.Contains(command.TitleFreeText), ct);
            return books;
        }
    }
}
