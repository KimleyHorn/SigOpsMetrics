using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SigOpsMetrics.API.Classes
{
    public static class GenericEnums
    {
        public enum FilteredItemType
        {
            Signal,
            Corridor
        }

        //todo: add 15 min and hour descriptions
        public enum DataAggregationType
        {
            [Description("qhr")]
            Min,
            [Description("hr")]
            Hour,
            [Description("dy")]
            Daily,
            [Description("wk")]
            Weekly,
            [Description("mo")]
            Monthly,
            [Description("qu")]
            Quarterly

        }

        public enum DateRangeType
        {
            PriorDay,
            PriorWeek,
            PriorMonth,
            PriorQuarter,
            PriorYear,
            Custom
            
        }

        public enum DataPullSource
        {
            S3,
            Google
        }
    }
}
