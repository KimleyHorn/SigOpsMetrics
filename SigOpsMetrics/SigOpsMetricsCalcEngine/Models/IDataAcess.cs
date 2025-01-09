using System.Collections.Concurrent;

namespace SigOpsMetricsCalcEngine.Core.Models
{
    public interface IDataAccess
    {
        #region Methods

        public Task<bool> Calc(ConcurrentBag<BaseEventLogModel> baseSignal);

        public Task<bool> SignalToDB(ConcurrentBag<BaseEventLogModel> signal);

        public Task<bool> SignalToCsv(ConcurrentBag<BaseEventLogModel> signal);

        public Task<ConcurrentBag<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate);

        public Task<bool> Process(ConcurrentBag<BaseEventLogModel> isFiltered, bool archiveFlag);


        #endregion
    }
}