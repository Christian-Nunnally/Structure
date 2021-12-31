using System;
using System.Diagnostics.Contracts;

namespace Structure
{
    [Serializable]
    public class RecordIntegerTaskItem : TaskItem
    {
        public int RecordedInteger { get; set; }

        public override void DoTask(StructureIO io, NodeTreeCollection<TaskItem> tree)
        {
            Contract.Requires(io != null);
            io.ReadInteger($"Record integer metric for: {Name}", RecordInteger);
            base.DoTask(io, tree);
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