using SigOpsMetricsCalcEngine.Core.DataAccess;

namespace SigOpsMetricsCalcEngine.Core.Calcs
{
    internal interface ICalc
    {
        public static abstract Task<bool> Run(List<DateTime> validDates, BaseDataAccessLayer sigModels, string dir,
            bool archiveFlag = false);
    }
}
