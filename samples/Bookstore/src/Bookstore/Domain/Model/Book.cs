using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Book : AggregateRootBase<Guid>
    {
        public string Name { get; private set; }
        public string Author { get; private set; }
        public int Year { get; private set; }
        public Money Price { get; private set; }

        private Book() { }

        private Book(Guid id, string name, string author, int year, Money price) : base(id)
        {
            Name = name;
            Author = author;
            Year = year;
            Price = price;
            Validate();
        }

        public static Book Create(string name, string author, int year, Money price)
        {
            return new Book(Guid.NewGuid(), name, author, year, price);
        }

        public void UpdatePrice(Money newPrice)
        {
            Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
        }

        private void Validate()
        {
            if (Id == Guid.Empty)
                throw new ArgumentException("Book ID cannot be empty.", nameof(Id));

            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Book name cannot be empty.", nameof(Name));

            if (string.IsNullOrWhiteSpace(Author))
                throw new ArgumentException("Author name cannot be empty.", nameof(Author));

            if (Year < 0)
                throw new ArgumentException("Year must be a positive value.", nameof(Year));

            if (Price is null)
                throw new ArgumentException("Price must be specified.", nameof(Price));
        }
    }
}
