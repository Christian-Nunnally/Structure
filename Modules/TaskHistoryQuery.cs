using Structure.Structure;
using Structure.TaskItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Modules
{
    internal class TaskHistoryQuery
    {
        public TimeSpan Range { get; set; } = new TimeSpan(30, 0, 0, 0);

        public TimeSpan AggregationRange { get; set; } = new TimeSpan(1, 0, 0, 0);

        public bool InterpolateEmptyRanges { get; set; }

        public Func<List<TaskItem>, double> AggregationMode { get; set; } = AggregationFunctions.CountAggregationFunction;

        public (string Name, IList<TaskItem> Data) DataSet { get; set; }

        public string SearchTerm { get; set; }

        public List<TaskItem> CopiedFromIds = new List<TaskItem>();

        public List<(string Label, double Value)> ComputeValues(Predicate<TaskItem> filter)
        {
            var tasks = DataSet.Data.Where(x => x.CompletedDate + Range > DateTime.Now && filter(x)).ToList();
            if (CopiedFromIds.Count > 0)
            {
                tasks = tasks.Where(t => CopiedFromIds.Any(x => x.ID == t.CopiedFromID)).ToList();
            }
            var values = new List<(string Label, double Value)>();
            for (var aggregationSearchPoint = new TimeSpan(); aggregationSearchPoint < Range; aggregationSearchPoint += AggregationRange)
            {
                var groupedTasks = tasks.Where(x => x.CompletedDate + aggregationSearchPoint > DateTime.Now).ToList();
                foreach (var task in groupedTasks)
                {
                    tasks.Remove(task);
                }
                var newValue = (!groupedTasks.Any() && InterpolateEmptyRanges)
                    ? ($"", 0)
                    : ($"-{aggregationSearchPoint.Days}d{aggregationSearchPoint.Hours}h", AggregationMode(groupedTasks));
                values.Insert(0, newValue);
            }

            if (InterpolateEmptyRanges) values = InterpolateEmptyValues(values);
            return values;
        }

        private static List<(string Label, double Value)> InterpolateEmptyValues(List<(string Label, double Value)> values)
        {
            var interpolatedValues = new List<(string Label, double Value)>();
            for (int i = 0; i < values.Count; i++)
            {
                var interpolatedValue = GetInterpolatedValue(values, i);
                interpolatedValues.Add(interpolatedValue);
            }
            return interpolatedValues;
        }

        private static (string Label, double Value) GetInterpolatedValue(List<(string Label, double Value)> values, int i)
        {
            return string.IsNullOrEmpty(values[i].Label)
                ? InterpolateValue(values, i)
                : (values[i].Label, values[i].Value);
        }

        private static (string Label, double Value) InterpolateValue(List<(string Label, double Value)> values, int indexToInterpolate)
        {
            var indexOfValueToTheLeft = FindIndexOfFirstNonEmptyValueToTheLeftOfIndex(values, indexToInterpolate);
            var indexOfValueToTheRight = FindIndexOfFirstNonEmptyValueToTheRightOfIndex(values, indexToInterpolate); ;

            if (indexOfValueToTheLeft == -1 && indexOfValueToTheRight == -1) return ("", 0);

            var valueToTheLeft = (indexOfValueToTheLeft != -1)
                ? values[indexOfValueToTheLeft].Value
                : values[indexOfValueToTheRight].Value;
            if (indexOfValueToTheLeft == -1) indexOfValueToTheLeft = indexToInterpolate;

            var valueToTheRight = (indexOfValueToTheRight != -1)
                ? values[indexOfValueToTheRight].Value
                : values[indexOfValueToTheLeft].Value;
            if (indexOfValueToTheRight == -1) indexOfValueToTheRight = indexToInterpolate;

            var leftDistance = indexToInterpolate - indexOfValueToTheLeft;
            var rightDistance = indexOfValueToTheRight - indexToInterpolate;
            if (leftDistance + rightDistance == 0) return ("", 0);
            var value = (valueToTheRight * leftDistance / (leftDistance + rightDistance)) + (valueToTheLeft * rightDistance / (leftDistance + rightDistance));
            return (values[indexToInterpolate].Label, value);
        }

        private static int FindIndexOfFirstNonEmptyValueToTheRightOfIndex(List<(string Label, double Value)> values, int index)
        {
            for (int i = index; i < values.Count; i++)
                if (!IsValueEmpty(values[i])) return i;
            return -1;
        }

        private static int FindIndexOfFirstNonEmptyValueToTheLeftOfIndex(List<(string Label, double Value)> values, int index)
        {
            for (int i = index; i >= 0; i--)
                if (!IsValueEmpty(values[i])) return i;
            return -1;
        }

        private static bool IsValueEmpty((string Label, double Value) value) => string.IsNullOrEmpty(value.Label);
    }
}