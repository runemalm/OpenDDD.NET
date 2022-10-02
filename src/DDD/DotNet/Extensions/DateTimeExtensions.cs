using System;

namespace DDD.DotNet.Extensions
{
    public static class DateTimeExtension
    {
        public static bool EqualIgnoreUs(this DateTime dt1, DateTime dt2)
        {
            return
                dt1.Second == dt2.Second &&
                dt1.Minute == dt2.Minute &&
                dt1.Day == dt2.Day &&
                dt1.Hour == dt2.Hour &&
                dt1.Month == dt2.Month &&
                dt1.Year == dt2.Year;
        }
    }
}
