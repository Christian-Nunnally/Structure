﻿using System;
using System.Collections.Generic;

namespace Structure
{
    [Serializable]
    public class RecordStringTaskItem : TaskItem
    {
        private static readonly List<RecordStringTaskItem> _recordedStringMetrics = new List<RecordStringTaskItem>();

        public string RecordedString { get; set; }

        public override void DoTask(PersistedTree<TaskItem> tree)
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

        private void RecordString(string metricName, string result)
        {
            RecordedString = result;
            _recordedStringMetrics.Add(this);
        }
    }
}