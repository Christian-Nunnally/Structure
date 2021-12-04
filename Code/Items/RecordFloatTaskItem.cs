using System;
using System.Collections.Generic;

namespace Structure
{
    [Serializable]
    public class RecordFloatTaskItem : ActionTaskItem
    {
        private static readonly List<RecordFloatTaskItem> _recordedFloatMetrics = new List<RecordFloatTaskItem>();

        public RecordFloatTaskItem() : base(NoOp)
        {
        }

        public float RecordedFloat { get; set; }

        public override void DoAction(PersistedTree<TaskItem> tree)
        {
            IO.Write($"Record float metric for: {Name}");
            IO.Read(s => RecordFloat(Name, s));
        }

        public override TaskItem Copy()
        {
            var copy = new RecordFloatTaskItem();
            CopyTo(copy);
            copy.RecordedFloat = RecordedFloat;
            return copy;
        }

        private static void NoOp(TaskItem _)
        {
        }

        private void RecordFloat(string metricName, string result)
        {
            if (float.TryParse(result, out var number))
            {
                RecordedFloat = number;
                _recordedFloatMetrics.Add(this);
            }
            else
            {
                IO.Write($"{result} is not a valid float.");
                IO.Read(s => RecordFloat(metricName, s));
            }
        }
    }
}