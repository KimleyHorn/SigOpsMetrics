using System;
using SigOpsMetricsCalcEngine;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetrics.API.Classes.DTOs
{
    /// <summary>
    /// 
    /// </summary>
    public class FlashEventDTO : BaseSignalModel
    {


        public FlashEventDTO()
        {
            SignalID = 0;
            EventCode = 0;
            EventParam = 0;
            Timestamp = DateTime.MinValue;
        }

    }
}
