using SigOpsMetricsCalcEngine.Models;
using System.Configuration;
using System.Data;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    internal class RampMeterDataAccessLayer : BaseDataAccessLayer, IDataAccess

    {
        internal static readonly List<long?> EventList = [131];
        /// <summary>
        /// Constructor for the RampMeterDataAccessLayer that takes in a list of signals
        /// </summary>
        /// <param name="sigModels">A list of signals represented by BaseEventLogModels</param>
        public RampMeterDataAccessLayer(List<BaseEventLogModel> sigModels)
        {
            SignalEvents = sigModels;
        }

        #region Write to CSV

        /// <summary>
        /// A method that writes all the filtered BaseEventLogModels to the correct CSV file
        /// </summary>
        /// <param name="events">An enumerable of BaseEventLogModels that will be written to the MySQL database</param>
        /// <returns>True if the operation is successful, false otherwise</returns>
        /// <exception cref="InvalidOperationException">An exception thrown when the MySQL writer fails</exception>
        public static async Task<bool> WriteFlashEventsToCSV(IEnumerable<BaseEventLogModel> events)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));

            foreach (var eventData in events)
            {
                dataTable.Rows.Add(
                    eventData.Timestamp,
                    eventData.SignalID,
                    eventData.EventCode,
                    eventData.EventParam

                );
            }

            try
            {
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                await WriteToErrorLog("FlashEventDataAccessLayer", "toMySQL", e);
                throw;
            }
        }

        #endregion Write to MySQL

        public Task<List<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Process(List<BaseEventLogModel> isFiltered)
        {
            throw new NotImplementedException();
        }
    }
}
