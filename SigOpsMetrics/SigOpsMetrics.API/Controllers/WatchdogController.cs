using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.DataAccess;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
        public WatchdogController(IOptions<AppConfig> settings, IConfiguration configuration) : base(settings, configuration)
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
                return await WatchdogDataAccessLayer.GetWatchdogData(SqlConnectionReader, data);
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetWatchdogData", ex);
                return null;
            }
        }
    }
}
