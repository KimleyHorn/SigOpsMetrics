using System.Collections.Generic;

namespace SigOpsMetrics.API.Classes.Internal
{
    public class FilteredItems
    {
        public List<string> Items { get; set; }
        public GenericEnums.FilteredItemType FilterType { get; set; }
    }
}
