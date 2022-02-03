﻿using System;
using System.Diagnostics.Contracts;

namespace Structure
{
    [Serializable]
    public class RecordStringTaskItem : TaskItem
    {
        public string RecordedString { get; set; }

        public override bool CanDoTask(StructureIO io)
        {
            Contract.Requires(io != null);
            io.Read(RecordString, KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
            return true;
        }

        public override TaskItem Copy()
        {
            var copy = new RecordStringTaskItem();
            CopyTo(copy);
            copy.RecordedString = RecordedString;
            return copy;
        }

        private void RecordString(string result)
        {
            // TODO: Handle escape characters.
            RecordedString = result;
        }
    }
}