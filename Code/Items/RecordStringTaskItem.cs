using System;
using System.Collections.Generic;

namespace Structure
{
    [Serializable]
    public class RecordStringTaskItem : ActionTaskItem
    {
        private static readonly List<RecordStringTaskItem> _recordedStringMetrics = new List<RecordStringTaskItem>();

        public RecordStringTaskItem() : base(NoOp)
        {
        }

        public string RecordedString { get; set; }

        public override void DoAction(PersistedTree<TaskItem> tree)
        {
            IO.Read(s => RecordString(Name, s));
        }

        public override TaskItem Copy()
        {
            var copy = new RecordStringTaskItem();
            CopyTo(copy);
            copy.RecordedString = RecordedString;
            return copy;
        }

        private static void NoOp(TaskItem _)
        {
        }

        private void RecordString(string metricName, string result)
        {
            RecordedString = result;
            _recordedStringMetrics.Add(this);
        }
    }
}