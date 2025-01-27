using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class LineItem : EntityBase<Guid>
    {
        public string BookName { get; private set; }
        public float Price {get; private set; }
        
        private LineItem() : base(Guid.Empty) { }

        public LineItem(Guid id, string bookName, float price) : base(id)
        {
            BookName = bookName;
            Price = price;
        }
    }
}
