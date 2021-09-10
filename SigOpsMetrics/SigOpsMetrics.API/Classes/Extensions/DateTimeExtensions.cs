using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        #endregion
    }
}
