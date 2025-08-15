using System.Text.RegularExpressions;

namespace orch.common
{
    public static class EtPhoneNumberUtil
    {
        public static string? GetE164(string? phoneNo)
        {
            return TryMultipleNumbers(phoneNo, number =>
            {
                // Remove all spaces and any other non-digit characters except + sign
                number = Regex.Replace(number, @"[^\d+]", "");

                // Handle various prefixes
                if (number.StartsWith("00251"))
                {
                    number = string.Concat("+251", number.AsSpan(5));
                }
                else if (number.StartsWith("0251"))
                {
                    number = string.Concat("+251", number.AsSpan(4));
                }
                else if (number.StartsWith("251"))
                {
                    // If it's exactly 12 digits (251 + 9 digits), validate the remaining 9 digits
                    if (number.Length == 12)
                    {
                        var remaining = number[3..];
                        if (!Regex.IsMatch(remaining, @"^[97]\d{8}$"))
                        {
                            return null;
                        }

                        number = string.Concat("+251", remaining);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (number.StartsWith("+251"))
                {
                    // Already in international format, just validate
                    if (!Regex.IsMatch(number, @"^\+251[97]\d{8}$"))
                    {
                        return null;
                    }
                }
                else if (number.StartsWith("0"))
                {
                    // Handle local format (0xxxxxxxxx)
                    if (!Regex.IsMatch(number, @"^0[97]\d{8}$"))
                    {
                        return null;
                    }

                    number = string.Concat("+251", number.AsSpan(1));
                }
                else if (Regex.IsMatch(number, @"^[97]\d{8}$"))
                {
                    // Just 9 digits starting with 9 or 7
                    number = "+251" + number;
                }
                else
                {
                    return null;
                }

                // Final validation to ensure the number is in the correct format
                return Regex.IsMatch(number, @"^\+251[97]\d{8}$") ? number : null;
            });
        }

        public static string? GetLocal(string? phoneNo)
        {
            return TryMultipleNumbers(phoneNo, number =>
            {
                // Try to get the international format first
                var internationalPhoneNo = GetE164(number);
                if (internationalPhoneNo == null)
                {
                    return null;
                }

                // Convert from +251xxxxxxxxx to 0xxxxxxxxx
                return string.Concat("0", internationalPhoneNo.AsSpan(4));
            });
        }
        
        private static string? TryMultipleNumbers(string? phoneNo, Func<string, string?> formatter)
        {
            if (string.IsNullOrWhiteSpace(phoneNo))
            {
                return null;
            }

            // Split by both forward and backward slashes
            var numbers = phoneNo.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            // Try each number until we find a valid one
            return numbers.Select(number => formatter(number.Trim())).OfType<string>().FirstOrDefault();
        }
    }
}