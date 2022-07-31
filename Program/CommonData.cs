using Structur.IO.Persistence;
using Structur.TaskItems;

namespace Structur.Program
{
    public class StructureData
    {
        public NodeTree<TaskItem> Tasks { get; } = new NodeTree<TaskItem>();

        public NodeTree<TaskItem> Routines { get; } = new NodeTree<TaskItem>();
    }
}