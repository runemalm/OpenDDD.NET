using System;

namespace OpenDDD.Infrastructure.Ports.MessageBroker
{
    public class ConsumerGroup : IEquatable<ConsumerGroup>
    {
        private readonly string _value;
        
        public ConsumerGroup(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }
        
        // IEquatable

        public bool Equals(ConsumerGroup? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value == other._value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConsumerGroup)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(ConsumerGroup? left, ConsumerGroup? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ConsumerGroup? left, ConsumerGroup? right)
        {
            return !Equals(left, right);
        }
    }
}
