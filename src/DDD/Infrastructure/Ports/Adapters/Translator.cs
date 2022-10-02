using System;

namespace DDD.Infrastructure.Ports.Adapters
{
	public class Translator
	{
		public Translator()
		{
			
		}

		public T MapEnum<T>(Enum value) where T : struct => MapEnum<T>(value.ToString());

		public T MapEnum<T>(string value) where T : struct
		{
			if (value == null)
				throw new InvalidCastException($"Cannot map null to {typeof(T)}");
			return Enum.TryParse<T>(value, true, out var result)
				? result
				: throw new InvalidCastException($"Cannot map {value} to {typeof(T)}");
		}
	}
}
