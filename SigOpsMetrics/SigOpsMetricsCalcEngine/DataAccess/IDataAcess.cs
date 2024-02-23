using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    internal interface IDataAccess
    {

        static List<long?> EventList { get; set; }
        static string? MySqlTableName { get; set; }
        public Task<List<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate);
        public Task<bool> Process(List<BaseEventLogModel> isFiltered);
        
    }
}
