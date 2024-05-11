using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
namespace orch.common
{

    public struct EthiopianDate
    {
        public int Day;
        public int Month;
        public int Year;
        public long GrigDate=>EthiopianDate.ToGrig(this);
        public DateTime DotNetDate => EthiopianDate.ToDotNetTime(this);
        public static bool IsLeapYearEt(int y)
        {
            return ((y % 4) == 3);
        }
        public static int EthiopianMonthLength(int m, int y)
        {
            if (m == 13)
            {
                return (IsLeapYearEt(y) ? 6 : 5);
            }
            return 30;
        }
        private static bool IsLeapYearGr(int year)
        {
            return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
        }
        public static int GetDayNoEthiopian(EthiopianDate etDate)
        {
            int num = etDate.Year / 4;
            int num2 = etDate.Year % 4;
            return (((((num * 1461) + (num2 * 365)) + ((etDate.Month - 1) * 30)) + etDate.Day) - 1);
        }
        private static int GrigorianMonthLength(int index, int year)
        {
            switch (index)
            {
                case 1:  // January
                case 3:  // March
                case 5:  // May
                case 7:  // July
                case 8:  // August
                case 10: // October
                case 12: // December
                    return 31;

                case 2:  // February
                    return IsLeapYearGr(year) ? 29 : 28;
            }
            return 30;  // April, June, September, November
        }
        public static DateTime GrigorianDateFromDayNo(int dayNum)
        {
            int year = 1, month = 1, day;

            // No need to add 1 to the dayNum

            int num400 = dayNum / 146097; // number of full 400-year periods
            dayNum %= 146097;
            if (dayNum == 0)
            {
                return new DateTime(400 * num400, 12, 31);
            }

            int num100 = Math.Min(dayNum / 36524, 3); // number of full 100-year periods, but not more than 3
            dayNum -= num100 * 36524;
            if (dayNum == 0)
            {
                return new DateTime(400 * num400 + 100 * num100, 12, 31);
            }

            int num4 = dayNum / 1461; // number of full 4-year periods
            dayNum %= 1461;
            if (dayNum == 0)
            {
                return new DateTime(400 * num400 + 100 * num100 + 4 * num4, 12, 31);
            }

            int num1 = Math.Min(dayNum / 365, 3); // number of full years, but not more than 3
            dayNum -= num1 * 365;
            if (dayNum == 0)
            {
                return new DateTime(400 * num400 + 100 * num100 + 4 * num4 + num1, 12, 31);
            }

            year += 400 * num400 + 100 * num100 + 4 * num4 + num1;

            // The remaining dayNum is the day of the year, we need to figure out which month and day of month it is
            while (true)
            {
                int daysInMonth = GrigorianMonthLength(month, year);

                if (dayNum <= daysInMonth)
                {
                    day = dayNum;
                    break;
                }

                dayNum -= daysInMonth;
                month++;
            }

            return new DateTime(year, month, day);
        }

        public EthiopianDate(int dn)
        {
            int num;
            int num2;
            int num3;
            int num4;

            num = dn / 1461;
            num2 = dn % 1461;
            num3 = num2 / 365;
            num4 = num2 % 365;
            if (num2 != 1460)
            {
                this.Year = (num * 4) + num3;
                this.Month = (num4 / 30) + 1;
                this.Day = (num4 % 30) + 1;
            }
            else
            {
                this.Year = ((num * 4) + num3) - 1;
                this.Month = 13;
                this.Day = 6;
            }
        }
        
        private static int AddGregorianMonths(int m, int y)
        {
            int sum = 0;
            for (int i = 1; i < m; i++)
            {
                sum += GrigorianMonthLength(i, y);
            }
            return sum;
        }
        public static int GetDayNoGrigorian(DateTime date)
        {
            int years = date.Year - 1; 
            int leap_years = years / 4 - years / 100 + years / 400; 
            int non_leap_years = years - leap_years; 
            int days_in_previous_years = leap_years * 366 + non_leap_years * 365;

            int days_in_current_year = AddGregorianMonths(date.Month, date.Year) + date.Day;
            return days_in_previous_years + days_in_current_year;
        }
        public EthiopianDate(int d, int m, int y)
        {
            this.Day = d;
            this.Month = m;
            this.Year = y;
        }

        public static EthiopianDate ToEth(DateTime dt)
        {
            return new EthiopianDate(GetDayNoGrigorian(dt) - 2431);
        }
        public static EthiopianDate ToEth(long time)
        {
            return ToEth(Helpers.LongToTime(time));
        }

        public static long ToGrig(EthiopianDate et)
        {
            return Helpers.TimeToLong(ToDotNetTime(et));
        }
        public static DateTime ToDotNetTime(EthiopianDate et)
        {
            return GrigorianDateFromDayNo(GetDayNoEthiopian(et) + 2431);
        }
        public static string GetEtMonthName(int m)
        {
            switch (m)
            {
                case 1:
                    return "መስከረም";

                case 2:
                    return "ጥቅምት";

                case 3:
                    return "ህዳር";

                case 4:
                    return "ታህሳስ";

                case 5:
                    return "ጥር";

                case 6:
                    return "የካቲት";

                case 7:
                    return "መጋቢት";

                case 8:
                    return "ሚያዚያ";

                case 9:
                    return "ግንቦት";

                case 10:
                    return "ሰኔ";

                case 11:
                    return "ሐምሌ";

                case 12:
                    return "ነሀሴ";

                case 13:
                    return "ጳጉሜ";
            }
            return "";
        }
        public static string GetDayOfWeekNameEt(int d)
        {
            switch (d)
            {
                case 1:
                    return "ሰኞ";

                case 2:
                    return "ማክሰኞ";

                case 3:
                    return "ረቡዕ";

                case 4:
                    return "ሀሙስ";

                case 5:
                    return "አርብ";

                case 6:
                    return "ቅዳሜ";

                case 7:
                    return "እሁድ";
            }
            return "";
        }

        public static string ToEtTimeString(long time)
        {
            var dateTime = Helpers.LongToTime(time);
            var hour = dateTime.Hour;
            var minute = dateTime.Minute;
            var second = dateTime.Second;

            // Convert the hour from the standard 24-hour format to Ethiopian time
            string period = hour >= 6 && hour < 18 ? "AM" : "PM"; // Determine AM/PM period
            hour = (hour + 6) % 12;
            hour = hour == 0 ? 12 : hour; // Adjust 0 to 12 for Ethiopian time

            // Format the time as a string in the format "hh:mm:ss AM/PM"
            return $"{hour:D2}:{minute:D2}";
        }


        public override string ToString()
        {
            return (this.Day.ToString("00") + "/" + this.Month.ToString("00") + "/" + this.Year.ToString("0000"));
        }

        public  string ToNamedMonthString()
        {
            return $"{GetEtMonthName(this.Month)} {this.Day}, {this.Year}";
        }

        public static bool IsValid(EthiopianDate date)
        {
            if ((date.Year < 1000) || (date.Year > 3000))
                return false;
         
            if (date.Month < 1)
                return false;

            if (date.Day < 1)
                return false;

            if (date.Month > 13)
                return false;
            
            if (date.Day > EthiopianMonthLength(date.Month, date.Year))
                return false;

            return true;
        }
        bool isValid()
        {
            if (Year < 1 || Year > 3000)
                return false;
            if (Month < 1 || Month > 13)
                return false;
            if (Month == 13)
            {
                if (IsLeapYearEt(Year))
                    return Day >= 1 && Day <= 6;
                else
                    return Day >= 1 && Day <= 5;
            }
            return Day >= 1 && Day <= 30;
        }
        public static EthiopianDate AddYears(EthiopianDate etDate, int years)
        {
            if (!etDate.isValid())
                throw new InvalidDataException($"Invalid ethiopian date {etDate.ToString()}");
            var newYear = etDate.Year + years;
            if (etDate.Month == 13 && etDate.Day == 6)
                if (!IsLeapYearEt(newYear))
                    return new EthiopianDate(5, etDate.Month, newYear);
            return new EthiopianDate(etDate.Day, etDate.Month, newYear);
        }

        public static EthiopianDate AddDays(EthiopianDate etDate, int days)
        {
            if (!etDate.isValid())
                throw new InvalidDataException($"Invalid ethiopian date {etDate.ToString()}");
            return new EthiopianDate(GetDayNoEthiopian(etDate)+days);
        }
        public static double EthiopanYearDifference(long d1, long d2, bool upperBoundInclusive)
        {
            var date1 = EthiopianDate.ToEth(d1);
            var date2 = EthiopianDate.ToEth(d2);
            if (upperBoundInclusive)
                date2 = EthiopianDate.AddDays(date2, 1);
            var dayNo1 = date1.Month * 30 + date1.Day;
            var dayNo2 = date2.Month * 30 + date2.Day;

            double years = date2.Year - date1.Year - (dayNo2 >= dayNo1 ? 0 : 1);
            years += (dayNo2 - dayNo1) / 365.0;
            return years;
        }
        public static int FullEthiopianYearDifference(long d1, long d2, bool upperBoundInclusive, out int remainder)
        {
            var date1 = EthiopianDate.ToEth(d1);
            var date2 = EthiopianDate.ToEth(d2);
            if (upperBoundInclusive)
                date2 = EthiopianDate.AddDays(date2, 1);


            var dayNo1 = date1.Month * 30 + date1.Day;
            var dayNo2 = date2.Month * 30 + date2.Day;

            var years = date2.Year - date1.Year - (dayNo2 >= dayNo1 ? 0 : 1);
            remainder = dayNo2 - dayNo1;
            return years;
        }
        public static EthiopianDate Parse(string date)
        {
            string[] strArray = date.Split(new char[] { '\\', '/', ' ', ',' });
            if (strArray.Length < 3)
            {
                throw new Exception("Invalid date fromat");
            }
            int d = int.Parse(strArray[0]);
            int m = int.Parse(strArray[1]);
            return new EthiopianDate(d, m, int.Parse(strArray[2]));
        }
        public static bool TryParse(string str, out EthiopianDate date)
        {
            string[] strArray = str.Split(new char[] { '\\', '/', ' ', ',' });
            date = new EthiopianDate();
            if (strArray.Length != 3)
            {
                return false;
            }

            int d;
            int m;
            int y;
            if (!int.TryParse(strArray[0], out d))
                return false;
            if (!int.TryParse(strArray[1], out m))
                return false;
            if (!int.TryParse(strArray[2], out y))
                return false;


            if (m < 1 || d < 1 || y < 1800)
                return false;
            if (m < 13)
            {
                if (d < 1 || d > 30)
                    return false;
            }
            if (m == 13)
            {
                if (IsLeapYearEt(y) && d > 6)
                    return false;
                if (d > 5)
                    return false;
            }
            if (m > 13)
                return false;
            if (y > 2100)
                return false;

            date = new EthiopianDate(d, m, y);
            return true;
        }

    }
}

