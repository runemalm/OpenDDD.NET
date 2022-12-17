using System;

namespace DDD.Application
{
	public class ActionId : IEquatable<ActionId>
	{
		public string Value;
		public ActionId() { }
		
		public static ActionId Create()
		{
			var actionId = new ActionId()
			{
				Value = Guid.NewGuid().ToString()
			};
			return actionId;
		}

		public static ActionId BootId()
		{
			var actionId = new ActionId
			{
				Value = "BootActionId"
			};
			return actionId;
		}

		public override string ToString()
		{
			return Value;
		}
		
		// Equality

		public bool Equals(ActionId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Value == other.Value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ActionId)obj);
		}

		public override int GetHashCode()
		{
			return (Value != null ? Value.GetHashCode() : 0);
		}

		public static bool operator ==(ActionId left, ActionId right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ActionId left, ActionId right)
		{
			return !Equals(left, right);
		}
	}
}
