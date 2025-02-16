using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Exception;
using Bookstore.Domain.Model;

namespace Bookstore.Domain.Service
{
    public class OrderDomainService : IOrderDomainService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepository<Book, Guid> _bookRepository;

        public OrderDomainService(
            ICustomerRepository customerRepository,
            IRepository<Book, Guid> bookRepository)
        {
            _customerRepository = customerRepository;
            _bookRepository = bookRepository;
        }

        public async Task<Order> PlaceOrderAsync(Guid customerId, Guid bookId, CancellationToken ct)
        {
            var customer = await _customerRepository.FindAsync(customerId, ct)
                ?? throw new EntityNotFoundException(nameof(Customer), customerId);

            var book = await _bookRepository.FindAsync(bookId, ct)
                ?? throw new EntityNotFoundException(nameof(Book), bookId);

            var order = Order.Create(customer.Id);
            order.AddLineItem(book.Id, book.Price);
            return order;
        }
    }
}
