using System;
using System.Diagnostics.Contracts;

namespace Structure
{
    [Serializable]
    public class RecordFloatTaskItem : TaskItem
    {
        public float RecordedFloat { get; set; }

        public override void DoTask(StructureIO io, NodeTreeCollection<TaskItem> tree)
        {
            Contract.Requires(io != null);
            io.Write($"Record float metric for: {Name}");
            io.Read(s => RecordFloat(io, Name, s), KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
            base.DoTask(io, tree);
        }

        public override TaskItem Copy()
        {
            var copy = new RecordFloatTaskItem();
            CopyTo(copy);
            copy.RecordedFloat = RecordedFloat;
            return copy;
        }

        private void RecordFloat(StructureIO io, string metricName, string result)
        {
            if (float.TryParse(result, out var number))
            {
                RecordedFloat = number;
            }
            else
            {
                io.Write($"{result} is not a valid float.");
                io.Read(s => RecordFloat(io, metricName, s), KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
            }
        }
    }
}