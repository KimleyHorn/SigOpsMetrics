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
            if (string.IsNullOrWhiteSpace(text))
                return 0.0;

            try
            {
                return Convert.ToDouble(text);
            }
            catch (Exception)
            {
                return 0.0;
            }
        }
    }
}
