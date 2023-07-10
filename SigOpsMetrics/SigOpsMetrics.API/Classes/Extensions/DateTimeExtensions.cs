using System;
using System.Collections.Generic;
using System.Linq;

namespace SigOpsMetrics.API.Classes.Extensions
{
    public static class DateTimeExtensions
    {
        #region Quarter Helpers

        public static string NearestQuarterEnd(this DateTime date)
        {
            date = date.AddMonths(3); //We want to round up, so add 3 months to get into the next quarter
            IEnumerable<DateTime> candidates =
                QuartersInYear(date.Year).Union(QuartersInYear(date.Year - 1));
            var result = candidates.Where(d => d < date.Date).OrderBy(d => d).Last();
            var quarterResult = Math.Ceiling((double)result.Month / 4);
            return result.Year + "." + quarterResult;
        }

        static IEnumerable<DateTime> QuartersInYear(int year)
        {
            return new List<DateTime>()
            {
                new DateTime(year, 3, 31),
                new DateTime(year, 6, 30),
                new DateTime(year, 9, 30),
                new DateTime(year, 12, 31),
            };
        }

        /// <summary>
        /// Converts a date stored as a string to a DateTime
        /// </summary>
        /// <param name="input"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DateTime ConvertQuarterStringToDateTime(this string input)
        {
            try
            {
                var year = input.Substring(0, 4);
                var quarter = input.Substring(5, 1);
                DateTime output = new DateTime(year.ToInt(), quarter.ToInt() * 3, 30);
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse the provided data to a date field. {input}");
            }
        }

        #endregion
    }
}
