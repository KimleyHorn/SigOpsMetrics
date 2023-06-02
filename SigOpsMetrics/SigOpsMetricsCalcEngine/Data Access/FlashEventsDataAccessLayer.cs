using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Internal;

namespace SigOpsMetricsCalcEngine.Data_Access
{
    public class FlashEventsDataAccessLayer : SigOpsMetrics.API.DataAccess.SignalsDataAccessLayer
    {



         static readonly FilterDTO _globalFilter = new()
        {
            city = "Atlanta",
            signalId = "106"
        };


         /// <summary>
         /// Uses the city and signal names from the _globalFilter object to get the flash events in
         /// Atlanta for signal 106 using the _globalFilter object 
         /// </summary>
         /// <param name="sqlConnection"></param>
         /// <returns></returns>
        public static async Task<FilteredItems> 
            GetFlashEvents(MySqlConnection sqlConnection)
        {
           
            var f = await GetSignalsByFilter(sqlConnection, _globalFilter);


            return f;
        }



        /// <summary>
        /// Uses the city and signal names from the _globalFilter object to get the flash events in
        /// Atlanta for signal 106 using a custom filter specified by the user
        ///
        /// This method is the template for the other GetFlashEvents methods
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static async Task<FilteredItems>
            GetFlashEvents(MySqlConnection sqlConnection, FilterDTO filter)
        {
 
            var f = await GetSignalsByFilter(sqlConnection, filter);

            return f;
        }
        
        
        /// <summary>
        /// Uses the city and signal names from the _globalFilter object to get the flash events in
        /// Atlanta for signal 106 using the date range specified by dateRange
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="dateRange"></param>
        /// <returns></returns>
        public static async Task<FilteredItems>
            GetFlashEvents(MySqlConnection sqlConnection, int dateRange)
        {
            _globalFilter.dateRange = dateRange;

            var f = await GetSignalsByFilter(sqlConnection, _globalFilter);

            return f;
        }
       
        
        
        // TODO: Add custom start and end times (if necessary)
        /// <summary>
        /// Uses the city and signal names from the _globalFilter object to get the flash events in
        /// Atlanta for signal 106 using the date range specified by customStart and customEnd
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="customStart"></param>
        /// <param name="customEnd"></param>
        /// <returns></returns>
        public static async Task<FilteredItems>
            GetFlashEvents(MySqlConnection sqlConnection, string customStart, string customEnd)
        {
            _globalFilter.customStart = customStart;
            _globalFilter.customEnd = customEnd;

            var f = await GetSignalsByFilter(sqlConnection, _globalFilter);

            return f;
        }
        
    }
}
