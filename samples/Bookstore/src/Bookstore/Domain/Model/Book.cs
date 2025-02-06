using OpenDDD.Domain.Model.Base;

namespace Bookstore.Domain.Model
{
    public class Book : AggregateRootBase<Guid>
    {
        public string Name { get; private set; }
        public string Author { get; private set; }
        public int Year { get; private set; }

        // private Book() : base(Guid.Empty)
        // {
        //     
        // }
        
        public static Book Create(string name, string author, int year)
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Name = name,
                Author = author,
                Year = year
            };

            book.Validate();
            return book;
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
        }
    }
}
