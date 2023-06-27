using System;

namespace OpenDDD.NET
{
	public interface IDateTimeProvider
	{
		DateTime Now { get; }
		void Reset();
		void Set(Func<DateTime> dateTimeNowFunc);
	}
}
