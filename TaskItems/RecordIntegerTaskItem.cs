using System;
using System.Collections.Generic;

namespace Structure
{
    [Serializable]
    public class RecordIntegerTaskItem : TaskItem
    {
        private static readonly List<RecordIntegerTaskItem> _recordedIntegerMetrics = new List<RecordIntegerTaskItem>();

        public int RecordedInteger { get; set; }

        public override void DoTask(PersistedTree<TaskItem> tree)
        {
            IO.Read(s => RecordInteger(Name, s));
        }

        public override TaskItem Copy()
        {
            var copy = new RecordIntegerTaskItem();
            CopyTo(copy);
            copy.RecordedInteger = RecordedInteger;
            return copy;
        }

        private void RecordInteger(string metricName, string result)
        {
            IO.Write($"Record integer metric for: {metricName}");
            if (int.TryParse(result, out var integer))
            {
                RecordedInteger = integer;
                _recordedIntegerMetrics.Add(this);
            }
            else
            {
                IO.Write($"{result} is not a valid integer.");
                IO.Read(s => RecordInteger(metricName, s));
            }
        }
    }
}