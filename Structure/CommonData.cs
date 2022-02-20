using Structure.IO.Persistence;
using Structure.TaskItems;

namespace Structure.Structure
{
    public class StructureData
    {
        public NodeTreeCollection<TaskItem> ActiveTaskTree { get; } = new NodeTreeCollection<TaskItem>();

        public NodeTreeCollection<TaskItem> Routines { get; } = new NodeTreeCollection<TaskItem>();
    }
}