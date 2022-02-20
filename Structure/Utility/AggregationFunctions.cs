using Structure.TaskItems;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Structure.Structure.Utility
{
    public static class AggregationFunctions
    {
        public static double CountAggregationFunction(List<TaskItem> list) => list?.Count ?? 0;

        public static double MaxAggregationFunction(List<TaskItem> list)
        {
            Contract.Requires(list != null);
            if (list.Count == 0) return 0;
            return list.Max(x => GetNumericValueOfItem(x));
        }

        public static double SumAggregationFunction(List<TaskItem> list)
        {
            Contract.Requires(list != null);
            if (list.Count == 0) return 0;
            return list.Sum(item => GetNumericValueOfItem(item));
        }

        public static double MinAggregationFunction(List<TaskItem> list)
        {
            Contract.Requires(list != null);
            if (list.Count == 0) return 0;
            return list.Min(item => GetNumericValueOfItem(item));
        }

        public static double MeanAggregationFunction(List<TaskItem> list)
        {
            Contract.Requires(list != null);
            if (list.Count == 0) return 0;
            return SumAggregationFunction(list) / list.Count;
        }

        public static double GetNumericValueOfItem(TaskItem item)
        {
            if (item is RecordFloatTaskItem floatTaskItem)
                return floatTaskItem.RecordedFloat;
            return item is RecordIntegerTaskItem integerTaskItem
                ? integerTaskItem.RecordedInteger : 0;
        }
    }
}
