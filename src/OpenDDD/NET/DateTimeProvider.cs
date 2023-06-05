using System;

namespace OpenDDD.NET
{
	public class DateTimeProvider
	{
		/*
         * DateTimeProvider is used to retrieve current time.
		 * It basically extends DateTime to be able to do "time jumps".
         */
		private static Func<DateTime> _dateTimeNowFunc = () => DateTime.UtcNow;
		public static DateTime Now => _dateTimeNowFunc();
		
		public static void Reset()
		{
			Set(() => DateTime.UtcNow);
		}

		public static void Set(Func<DateTime> dateTimeNowFunc)
		{
			_dateTimeNowFunc = dateTimeNowFunc;
		}
	}
}
