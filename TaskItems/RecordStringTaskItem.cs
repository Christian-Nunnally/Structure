using System;
using System.Diagnostics.Contracts;

namespace Structure
{
    [Serializable]
    public class RecordStringTaskItem : TaskItem
    {
        public string RecordedString { get; set; }

        public override void DoTask(StructureIO io, NodeTreeCollection<TaskItem> tree)
        {
            Contract.Requires(io != null);
            io.Read(RecordString, KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
            base.DoTask(io, tree);
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