using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SigOpsMetrics.API.Controllers
{
    /// <summary>
    /// Controller for signals, corridors, and groups
    /// </summary>
    [ApiController]
    [Route("watchdog")]
    public class WatchdogController : _BaseController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="connection"></param>
        /// <param name="cache"></param>
        public WatchdogController(IOptions<AppConfig> settings, MySqlConnection connection, IMemoryCache cache) : base(settings, connection, cache)
        {
        }


        /// <summary>
        /// Endpoint for submitting watchdog filter and retrieving results.
        /// </summary>
        /// <returns></returns>
        [HttpPost("data")]
        public async Task<List<WatchdogHeatmapDTO>> Data(WatchdogFilterRequestObject data)
        {
            try
            {
                return await WatchdogDataAccessLayer.GetWatchdogData(SqlConnection, data);
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "ContactUs", ex);
                return null;
            }
        }
    }
}
