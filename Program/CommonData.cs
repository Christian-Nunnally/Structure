using Structure.IO.Persistence;
using Structure.TaskItems;

namespace Structure.Program
{
    public class StructureData
    {
        public NodeTree<TaskItem> Tasks { get; } = new NodeTree<TaskItem>();

        public NodeTree<TaskItem> Routines { get; } = new NodeTree<TaskItem>();
    }
}