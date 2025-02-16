using System.Globalization;
using OpenDDD.Domain.Model;

namespace Bookstore.Domain.Model
{
    public class Money : IValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }
        
        private Money() { }

        private Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency must be specified.", nameof(currency));

            Amount = amount;
            Currency = currency.ToUpper(CultureInfo.InvariantCulture);
        }
        
        public static Money USD(decimal amount) => new Money(amount, "USD");

        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal factor)
        {
            if (factor < 0)
                throw new ArgumentException("Factor cannot be negative.", nameof(factor));

            return new Money(Amount * factor, Currency);
        }

        public Money Divide(decimal divisor)
        {
            if (divisor <= 0)
                throw new ArgumentException("Divisor must be greater than zero.", nameof(divisor));

            return new Money(Amount / divisor, Currency);
        }

        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot perform operations on Money with different currencies.");
        }

        public override string ToString() => $"{Amount} {Currency}";

        public static implicit operator decimal(Money money) => money.Amount;

        public static Money operator +(Money a, Money b) => a.Add(b);
        public static Money operator -(Money a, Money b) => a.Subtract(b);
        public static Money operator *(Money a, decimal factor) => a.Multiply(factor);
        public static Money operator /(Money a, decimal divisor) => a.Divide(divisor);
    }
}
