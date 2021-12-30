using System;

namespace Structure
{
    [Serializable]
    public class RecordStringTaskItem : TaskItem
    {
        public string RecordedString { get; set; }

        public override void DoTask(StructureIO io, NodeTreeCollection<TaskItem> tree)
        {
            io.Read(RecordString);
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
            RecordedString = result;
        }
    }
}