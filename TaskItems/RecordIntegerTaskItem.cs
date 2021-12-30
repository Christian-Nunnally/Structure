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
            IO.ReadInteger($"Record integer metric for: {Name}", RecordInteger);
            base.DoTask(tree);
        }

        public override TaskItem Copy()
        {
            var copy = new RecordIntegerTaskItem();
            CopyTo(copy);
            copy.RecordedInteger = RecordedInteger;
            return copy;
        }

        private void RecordInteger(int result)
        {
            RecordedInteger = result;
            _recordedIntegerMetrics.Add(this);
        }
    }
}