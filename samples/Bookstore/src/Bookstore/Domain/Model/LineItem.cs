using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class LineItem : EntityBase<Guid>
    {
        public Guid BookId { get; private set; }
        public float Price { get; private set; }

        private LineItem(Guid id, Guid bookId, float price) : base(id)
        {
            BookId = bookId;
            Price = price;
            Validate();
        }

        public static LineItem Create(Guid bookId, float price)
        {
            return new LineItem(Guid.NewGuid(), bookId, price);
        }

        private void Validate()
        {
            if (Id == Guid.Empty)
                throw new ArgumentException("Line item ID cannot be empty.", nameof(Id));

            if (BookId == Guid.Empty)
                throw new ArgumentException("Book ID cannot be empty.", nameof(BookId));

            if (Price <= 0)
                throw new ArgumentException("Price must be greater than zero.", nameof(Price));
        }
    }
}
