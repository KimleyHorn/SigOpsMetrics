using System.Collections.Concurrent;

namespace SigOpsMetricsCalcEngine.Models
{
    internal interface IDataAccess
    {
        #region Methods

        public Task<ConcurrentBag<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate);

        public Task<bool> Process(ConcurrentBag<BaseEventLogModel> isFiltered);

        #endregion
    }
}