using System.Configuration;
using System.Data;
using Amazon.S3;
using Amazon.S3.Model;
using MySqlConnector;
using Parquet;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class FlashEventDataAccessLayer : BaseDataAccessLayer
    {

        
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["FLASH_EVENT_TABLE_NAME"];
        private static readonly string? MySqlFlashPairTableName = ConfigurationManager.AppSettings["FLASH_PAIR_TABLE_NAME"];


        #region Write to MySQL
        public static async Task<bool> WriteFlashEventsToDb(IEnumerable<BaseEventLogModel> events)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));

            // Populate the DataTable with events data
            foreach (var eventData in events)
            {
                dataTable.Rows.Add(
                    eventData.Timestamp,
                    eventData.SignalID,
                    eventData.EventCode,
                    eventData.EventParam

                );
            }
            // Open a connection to MySQL
            try
            {
                return await MySqlWriter(MySqlTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                //await WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
        }
        #endregion





        public static Task ConvertToFlash(List<BaseEventLogModel> signalList, DateTime startDate, DateTime endDate)
            {
                SignalEvents = new List<BaseEventLogModel>();

                foreach (var flash in signalList.Where(x => x.EventCode is 173 && x.Timestamp >= startDate && x.Timestamp <= endDate))
                {


                    FlashEvents.Add(new BaseEventLogModel
                    {
                        SignalID = flash.SignalID,
                        Timestamp = flash.Timestamp,
                        EventCode = flash.EventCode,
                        EventParam = flash.EventParam
                    });
                }

                return Task.CompletedTask;
            }




     



    }
}
