using OpenDDD.Domain.Model.ValueObject;

namespace MyBoundedContext.Domain.Model.Property
{
    public class Price : ValueObject
    {
        public double Amount { get; set; }
        public Currency Currency { get; set; }
        
        public static Price Create(double amount, Currency currency)
        {
            var price = 
                new Price
                {
                    Amount = amount,
                    Currency = currency
                };

            return price;
        }
    }
}
