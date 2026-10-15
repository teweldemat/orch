using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.common
{
    public static class DateHelpers
    {
        public static long AddDays(long time,int days)
        {
            return Helpers.TimeToLong(Helpers.LongToTime(time).AddDays(days));
        }

        public static decimal DaysDiff(long time1, long time2)
        {
            return ((decimal)Helpers.LongToTime(time2).Subtract(Helpers.LongToTime(time1)).Ticks)/
                (10_000L*1_1000L*24L*60L*60L);
        }

        public static long MilBetween(long from, long to)
        {
            return (long)Helpers.LongToTime(to).Subtract(Helpers.LongToTime(from)).TotalMilliseconds;
        }

        /// <summary>
        /// Selected calendar days, inclusive. The end instant is the start of the day after
        /// <paramref name="toDate"/>, so an end-of-day timestamp does not pull in the next day.
        /// </summary>
        public static CalendarDayRange ToCalendarDayRange(long fromDate, long toDate)
        {
            var fromInclusive = Helpers.LongToTime(fromDate).Date.TimeToLong();
            var toInclusive = Helpers.LongToTime(toDate).Date.TimeToLong();
            if (toInclusive < fromInclusive)
                throw new ArgumentException("toDate must be greater than or equal to fromDate");

            return new CalendarDayRange(fromInclusive, toInclusive, AddDays(toInclusive, 1));
        }
    }

    public readonly record struct CalendarDayRange(long FromInclusive, long ToInclusive, long ToExclusive);
}
