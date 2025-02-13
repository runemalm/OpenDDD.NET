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
        }

        public static LineItem Create(Guid bookId, float price)
        {
            return new LineItem(Guid.NewGuid(), bookId, price);
        }
    }
}
