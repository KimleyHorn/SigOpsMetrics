using System;
using SigOpsMetricsCalcEngine;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetrics.API.Classes.DTOs
{
    public class FlashEventDTO : FlashPairModel
    {

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public long? SignalID { get; set; }
        public long? duration { get; set; }
        public long? startParam { get; set; }

        public bool isOpen { get; set; }

        public FlashEventDTO()
        {

        }

        public FlashEventDTO(FlashEventModel flashStart, FlashEventModel flashEnd, long? signalId, long? startParam, bool isOpen) : base(flashStart, flashEnd, signalId, startParam, isOpen)
        {


        }

        public FlashEventDTO(long? startParam, long? signalId) : base(startParam, signalId)
        {
        }
    }
}
