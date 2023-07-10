using System;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetrics.API.Classes.DTOs
{
    /// <summary>
    /// 
    /// </summary>
    public class FlashEventDTO : BaseEventLogModel
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
