using System;

namespace OpenDDD.NET
{
	public class DateTimeProvider : IDateTimeProvider
	{
		/*
	     * DateTimeProvider is used to retrieve current time.
		 * It basically extends DateTime to be able to do "time jumps".
	     */
		private Func<DateTime> _dateTimeNowFunc = () => DateTime.UtcNow;
		public DateTime Now => _dateTimeNowFunc();

		public void Reset()
		{
			Set(() => DateTime.UtcNow);
		}

		public void Set(Func<DateTime> dateTimeNowFunc)
		{
			_dateTimeNowFunc = dateTimeNowFunc;
		}
	}
}
