using System.Collections.Concurrent;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal interface ICalc
    {
        public static Task<bool> Run(List<DateTime> validDates, ConcurrentBag<BaseEventLogModel> sigModels, string dir)
        {
            
        }
    }
}
