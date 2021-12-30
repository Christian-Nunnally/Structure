using System.Collections.Generic;

namespace Structure
{
    public class TaskExecutor : TaskPicker
    {
        protected readonly NodeTreeCollection<TaskItem> _tree;
        private readonly StructureIO _io;

        public TaskExecutor(
            StructureIO io,
            string prompt,
            NodeTreeCollection<TaskItem> tree)
            : base(io, prompt, "Complete", true, false, false, tree)
        {
            _tree = tree;
            PickedAction = CompleteTask;
            _io = io;
        }

        public virtual void CompleteTask(TaskItem task)
        {
            _io.Run(() => task?.DoTask(_io, _tree));
        }
    }
}