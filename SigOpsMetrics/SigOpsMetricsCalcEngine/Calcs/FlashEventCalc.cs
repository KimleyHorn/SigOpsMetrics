using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigOpsMetricsCalcEngine.DataAccess;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class FlashEventCalc
    {

        private static readonly string MySqlTableName = ConfigurationManager.AppSettings["FLASH_EVENT_TABLE_NAME"]  ?? "flash_event_log";
        private static readonly string MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"] ?? "test";



        public static async Task<bool> RunFlash(DateTime firstStart, DateTime endDate)
        {
            if (endDate < firstStart)
                throw new ArgumentException("Start date must be before end date");
            

            await BaseDataAccessLayer.CheckDB(MySqlTableName, "Timestamp", MySqlDbName, firstStart, endDate);

           
            //Preempt event list to check for eventCodes from SignalEvents list
            var flashEventList = new List<long?> { 173 };
            var validDates = BaseDataAccessLayer.FillData(firstStart, endDate, flashEventList);
            if (BaseDataAccessLayer.HasGaps(validDates))
            {
                Console.WriteLine("Gaps in data");
                return await BaseDataAccessLayer.ProcessEvents(validDates, "flash_event_log", eventCodes: flashEventList);

            }

            var startDate = await BaseDataAccessLayer.GetLastDay("flash_event_log", "Timestamp", MySqlDbName);

            if (startDate == default)
            {
                startDate = firstStart;
            }

            if (startDate > endDate)
            {
                //TODO: fix this error
                throw new ArgumentException("Start date must be before end date");
            }
            //This checks to see if the events are already in the database or if they are already stored in memory
            //if (!CheckList(startDate, endDate, flashEventList) &&
            //    !await CheckDB("flash_event_log", "Timestamp", MySqlDbName, startDate, endDate))
            //{
            //    //If events within date range are not within the database or memory, then grab the events from Amazon S3
            //    return await ProcessEvents(startDate, endDate, "flash_event_log", eventCodes: flashEventList);
            //}

            return await BaseDataAccessLayer.ProcessEvents(BaseDataAccessLayer.FillData(startDate, endDate, flashEventList), "flash_event_log", eventCodes: flashEventList);
        }

    }
}
