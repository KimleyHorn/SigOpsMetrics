using System;

namespace SigOpsMetrics.API.Classes.Extensions
{
    public static class StringExtensions
    {
        public static DateTime? ToNullableDateTime(this string text)
        {
            return DateTime.TryParse(text, out var date) ? date : (DateTime?) null;
        }
    }
}
