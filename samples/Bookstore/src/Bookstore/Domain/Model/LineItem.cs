using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class LineItem : EntityBase<Guid>
    {
        public Guid BookId { get; private set; }
        public Money Price { get; private set; }
        
        private LineItem() { }

        private LineItem(Guid id, Guid bookId, Money price) : base(id)
        {
            BookId = bookId;
            Price = price;
        }

        public static LineItem Create(Guid bookId, Money price)
        {
            return new LineItem(Guid.NewGuid(), bookId, price);
        }
    }
}
