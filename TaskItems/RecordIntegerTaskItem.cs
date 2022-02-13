using Structure.IO;
using System;
using System.Diagnostics.Contracts;

namespace Structure.TaskItems
{
    [Serializable]
    public class RecordIntegerTaskItem : TaskItem
    {
        public int RecordedInteger { get; set; }

        public override bool CanDoTask(StructureIO io)
        {
            Contract.Requires(io != null);
            io.ReadInteger($"Record integer metric for: {Name}", RecordInteger);
            return true;
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
        }
    }
}