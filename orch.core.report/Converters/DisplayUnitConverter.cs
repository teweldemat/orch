using System.Globalization;
using System.Text.RegularExpressions;

namespace orch.core.report.Converters
{
    public static class DisplayUnitConverter

    {
        private const int Dpi = 96; // Standard screen resolution in dots per inch

        public static string ConvertFromPixels(int pixels, string targetUnit)
        {
            return targetUnit.ToLower() switch
            {
                "in" => ConvertPixelsToInches(pixels).ToString("N2", CultureInfo.InvariantCulture) + "in",
                "cm" => ConvertPixelsToCentimeters(pixels).ToString("N2", CultureInfo.InvariantCulture) + "cm",
                "mm" => ConvertPixelsToMillimeters(pixels).ToString("N2", CultureInfo.InvariantCulture) + "mm",
                _ => throw new ArgumentException("Unrecognized unit. Only 'in', 'cm', and 'mm' are supported."),
            };
        }

        public static int ConvertToPixels(string source)
        {
            // Regex to extract the number and unit from the input string (e.g., "8.5in", "21cm")
            var match = Regex.Match(source.Trim(), @"^(\d+(\.\d+)?)(in|cm|mm)$");

            if (!match.Success)
            {
                throw new ArgumentException("Width format is not recognized. Please specify in the format of '<number><unit>', e.g., '8.5in', '21cm', or '200mm'.");
            }

            // Extract the numeric value and unit from the match
            var value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var unit = match.Groups[3].Value;

            // Convert the value to pixels based on the unit
            return unit switch
            {
                "in" => (int)(value * Dpi),// 96 pixels/inch
                "cm" => (int)(value * Dpi / 2.54),// 2.54 cm/inch
                "mm" => (int)(value * Dpi / 25.4),// 25.4 mm/inch
                _ => throw new ArgumentException("Unrecognized unit. Only 'in', 'cm', and 'mm' are supported."),
            };
        }

        private static double ConvertPixelsToInches(int pixels)
        {
            return pixels / (double)Dpi;
        }

        private static double ConvertPixelsToCentimeters(int pixels)
        {
            return (pixels / (double)Dpi) * 2.54; // 1 inch = 2.54 cm
        }

        private static double ConvertPixelsToMillimeters(int pixels)
        {
            return ConvertPixelsToCentimeters(pixels) * 10; // 1 cm = 10 mm
        }
        public static (double? value, string? unit) Parse(string input)
        {
            var match = Regex.Match(input, @"(\d+(\.\d+)?)\s*(in|cm|mm)");

            if (match.Success)
            {
                double value = double.Parse(match.Groups[1].Value);
                string unit = match.Groups[3].Value;
                return (value, unit);
            }

            return (null, null);
        }
    }
}
