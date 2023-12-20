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
    }
}
