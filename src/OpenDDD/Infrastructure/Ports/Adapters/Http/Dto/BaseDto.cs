using System;

namespace OpenDDD.Infrastructure.Ports.Adapters.Http.Dto
{
	public class BaseDto
	{
		public T ToEnum<T>(Enum value) where T : struct => ToEnum<T>(value.ToString());

		public T ToEnum<T>(string value) where T : struct
		{
			if (value == null)
				throw new InvalidCastException($"Cannot translate null to enum {typeof(T)}");
			return Enum.TryParse<T>(value, true, out var result)
				? result
				: throw new InvalidCastException($"Cannot translate {value} to enum {typeof(T)}");
		}
	}
}
