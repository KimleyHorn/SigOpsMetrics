using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    internal interface IDataAccess
    {
        #region Methods

        public Task<List<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate);

        public Task<bool> Process(List<BaseEventLogModel> isFiltered);

        #endregion
    }
}