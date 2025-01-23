using OpenDDD.Domain.Model;

namespace Bookstore.Domain.Model
{
    public class Item : IValueObject
    {
        public string BookName;
        public float Price;

        public Item(string bookName, float price)
        {
            BookName = bookName;
            Price = price;
        }
    }
}
