using System.Globalization;
using System.Text.RegularExpressions;

namespace orch.common
{
    public static class Helpers
    {
        public static long DoubleToMoney(double money)
        {
            return (long)Math.Round(money * 10000);
        }
        public static double MoneyToDouble(long money)
        {
            return (double)money / 10000;
        }
        public static long TimeToLong(this DateTime time)
        {
            //YYYYMMDDHHSSMMM
            return (((((
                (long)time.Year * 100
                + (long)time.Month) * 100
                + (long)time.Day) * 100
                + (long)time.Hour) * 100
                + (long)time.Minute) * 100
                + (long)time.Second) * 1000
                + (long)time.Millisecond;
            //long milliseconds = (long)(time - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            // return milliseconds;
        }
        public static long AddDaysToLongTime(this long time, int days)
        {
            return time.LongToTime().AddDays(days).TimeToLong();
        }

        public static long TimeToLongWithOutSeccond(DateTime time)
        {
            //YYYYMMDDHHSSMMM
            return (((
                (long)time.Year * 100
                + (long)time.Month) * 100
                + (long)time.Day) * 100
                + (long)time.Hour) * 100

               ;
            //long milliseconds = (long)(time - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            // return milliseconds;
        }
        public static DateTime LongToTime(this long time)
        {

            var ms = time % 1000;
            time = time / 1000;
            var second = time % 100;
            time = time / 100;
            var minute = time % 100;
            time = time / 100;
            var hour = time % 100;
            time = time / 100;
            var day = time % 100;
            time = time / 100;
            var month = time % 100;
            time = time / 100;
            var year = time;
            return new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);

            // return  DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime;
        }
        public static bool IsFullRegexMatch(string value, string pattern)
        {
            if (String.IsNullOrEmpty(value))
                return false;
            var m = System.Text.RegularExpressions.Regex.Match(value, pattern);
            return m != null && m.Length == value.Length;
        }
        /// <summary>
        /// Returns the type name. If this is a generic type, appends
        /// the list of generic type arguments between angle brackets.
        /// (Does not account for embedded / inner generic arguments.)
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        public static string GetFormattedName(this Type type)
        {
            if (type.IsGenericType)
            {
                string genericArguments = type.GetGenericArguments()
                                    .Select(x => x.Name)
                                    .Aggregate((x1, x2) => $"{x1}, {x2}");
                return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
                     + $"<{genericArguments}>";
            }
            return type.Name;
        }
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static T? CloneWithJson<T>(T obj) where T : class
        {
            if (obj == null)
                return null;
            return System.Text.Json.JsonSerializer.Deserialize<T>(
                System.Text.Json.JsonSerializer.Serialize(obj));
        }

        public static long DecimalToLong(decimal value)
        {
            // Multiply the decimal value by 100 to get the payroll long value
            // and then round it to the nearest integer.
            var x = Math.Round(value, 4);// MidpointRounding.AwayFromZero);
            return (long)Math.Round(x * 10000, MidpointRounding.AwayFromZero);
        }
        public static decimal LongToDecimal(long value)
        {
            // Divide the payroll long value by 100 to get the decimal value.
            return (decimal)value / 10000;
        }
        /// <summary>
        /// Converts a string to snake_case.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The snake_case representation of the input string.</returns>
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Convert the string to snake_case using regular expressions
            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        public static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
            return textInfo.ToTitleCase(Regex.Replace(input, "([A-Z])", " $1").ToLower()).Trim();
        }

        /// <summary>
        /// Returns the entity name with the "DAL" prefix removed.
        /// </summary>
        /// <param name="entityName">The entity name.</param>
        /// <returns>The entity name with the "DAL" prefix removed.</returns>
        public static string StripDALPrefix(string entityName)
        {
            const string prefix = "DAL";
            return entityName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? entityName[prefix.Length..] : entityName;
        }


        public static string AmmountToWords(decimal currency)
        {
            var currencyStr = currency.ToString();
            if (currencyStr.IndexOf(".") > -1)
            {
                var m = currencyStr.IndexOf(".");
                var n = currencyStr.Length - 1;
                if (Convert.ToDecimal(currencyStr.Substring(currencyStr.IndexOf(".") + 1)) > 0)
                    currencyStr = " and " + (Int32.Parse(currencyStr.Substring(currencyStr.IndexOf(".") + 1)) * 10).ToString().Substring(0, 2) + "/100 ";

            }
            else
            {
                currencyStr = "";
            }
            string[] ones = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            string[] tens = { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            string[] thousands = { "", "thousand", "million", "billion", "trillion" };

            if (currency == 0)
                return "zero ";
            if (currency < 1)
                return "zero " + currencyStr;
            if (currency < 0)
                return "minus " + AmmountToWords(Math.Abs(currency));

            string words = "";

            int thousandsIndex = 0;

            while (currency > 0)
            {
                int segment = (int)(currency % 1000);

                if (segment > 0)
                {
                    string segmentWords = "";

                    if (segment < 20)
                        segmentWords = ones[segment];
                    else if (segment < 100)
                        segmentWords = tens[segment / 10] + ((segment % 10 > 0) ? " " + ones[segment % 10] : "");
                    else
                        segmentWords = ones[segment / 100] + " hundred" + ((segment % 100 > 0) ? "  " + ((segment % 100 < 20) ? ones[segment % 100] : tens[(segment % 100) / 10] + ((segment % 10 > 0) ? " " + ones[segment % 10] : "")) : "");

                    words = segmentWords + " " + thousands[thousandsIndex] + " " + words;
                }

                currency /= 1000;
                thousandsIndex++;
            }
            var amountInWords = (words.Trim() + " Birr " + currencyStr);
            return amountInWords.Substring(0, 1).ToUpper() + amountInWords.Substring(1);
        }

        public static string NumberToAmharic(decimal num)
        {
            string[] ones =
            {
                "", "አንድ", "ሁለት", "ሶስት", "አራት", "አምስት", "ስድስት", "ሰባት", "ስመንት", "ዘጠኝ"
            };
            string[] tens =
            {
                "", "አስር", "ሃያ", "ሰላሳ", "አርባ", "አምሳ", "ስልሳ", "ሰባ", "ሰማንያ", "ዘጠና"
            };
            const string hundred = "መቶ";
            const string thousand = "ሺህ";
            const string million = "ሚሊዮን";
            const string billion = "ቢሊዮን";
            const string cent = "ሳንቲም";

            var birrs = Math.Floor(num);
            var centsDecimal = Math.Round((num - birrs) * 100);
            if (centsDecimal == 100)
            {
                birrs += 1;
                centsDecimal = 0;
            }
            var cents = (int)centsDecimal;

            if (cents > 0)
            {
                return NumberToAmharic(birrs) + " ና " + NumberToAmharic(cents) + " " + cent;
            }

            switch (num)
            {
                case < 10 and >= 1:
                    return ones[(int)Math.Floor(num)];
                case < 100:
                    return tens[(int)Math.Floor(num / 10)] +
                           (num % 10 != 0 ? " " + ones[(int)Math.Floor(num % 10)] : "");
                case < 1000:
                    {
                        var hundreds = (int)Math.Floor(num / 100);
                        var remainder = num % 100;
                        return ones[hundreds] + " " + hundred + (remainder != 0 ? " " + NumberToAmharic(remainder) : "");
                    }
                case < 1_000_000:
                    {
                        var thousands = (int)Math.Floor(num / 1000);
                        var remainder = num % 1000;
                        return NumberToAmharic(thousands) + " " + thousand +
                               (remainder != 0 ? " " + NumberToAmharic(remainder) : "");
                    }
                case < 1_000_000_000:
                    {
                        var millions = (int)Math.Floor(num / 1_000_000);
                        var remainder = num % 1_000_000;
                        return NumberToAmharic(millions) + " " + million +
                               (remainder != 0 ? " " + NumberToAmharic(remainder) : "");
                    }
                case < 1_000_000_000_000:
                    {
                        var billions = (int)Math.Floor(num / 1_000_000_000);
                        var remainder = num % 1_000_000_000;
                        return NumberToAmharic(billions) + " " + billion +
                               (remainder != 0 ? " " + NumberToAmharic(remainder) : "");
                    }
            }

            if (num % 1 != 0)
            {
                var decimalNumber = (int)Math.Round((num % 1) * 100);
                return NumberToAmharic(Math.Floor(num)) + " ና " + NumberToAmharic(decimalNumber) + " " + cent;
            }

            return num.ToString();
        }
        public static string EscapeStringForCsvField(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";

            var cleaned = Regex.Replace(str, @"\s+", " ").Trim(); // collapses all whitespace
            var escaped = cleaned.Replace("\"", "\"\"");           // escape quotes
            return $"\"{escaped}\"";
        }
    }


    public static class PropMapper
    {
        public static void MapFromBase<T>(this object data, T baseData)
        {
            if (baseData == null)
                return;
            var t = typeof(T);
            foreach (var prop in t.GetProperties()
                .Where(x => x.GetMethod != null
                            && !x.GetMethod.IsStatic
                            && x.GetMethod.GetParameters().Length == 0
                            && x.GetMethod.IsPublic
                            && x.SetMethod != null
                            )
                )
                prop.SetValue(data, prop.GetValue(baseData));
            foreach (var f in t.GetFields()
                .Where(x => !x.IsStatic
                            && x.IsPublic
                            && !x.IsLiteral
                            ))
                f.SetValue(data, f.GetValue(baseData));
        }
    }



}
