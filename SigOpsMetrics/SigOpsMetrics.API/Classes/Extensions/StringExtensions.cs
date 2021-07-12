using System;

namespace SigOpsMetrics.API.Classes.Extensions
{
    public static class StringExtensions
    {
        public static DateTime? ToNullableDateTime(this string text)
        {
            return DateTime.TryParse(text, out var date) ? date : (DateTime?) null;
        }

        public static double ToDouble(this string text)
        {
            return !double.TryParse(text, out var retVal) ? 0.0 : retVal;
        }

        public static bool IsStringNullOrBlank(this string val)
        {
            return string.IsNullOrWhiteSpace(val) || val.ToLower() == "null";
        }
        
        public static double ToDouble(this object input)
        {
            return !double.TryParse(input?.ToString(), out double retVal) ? 0.0 : retVal;
        }
    }
}
