using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;

namespace Structure.Editors
{
    public class TaskExecutor
    {
        private readonly StructureIO _io;
        private readonly NodeTree<TaskItem> _tree;
        public ItemPicker<TaskItem> ItemPicker { get; }

        public TaskExecutor(StructureIO io, string prompt, NodeTree<TaskItem> tree, bool allowInserting)
        {
            ItemPicker = new ItemPicker<TaskItem>(io, prompt, false, false, tree, allowInserting);
            ItemPicker.SetPickAction(RunCompleteTask);
            _io = io;
            _tree = tree;
        }

        public virtual void RunCompleteTask(TaskItem task) =>  _io.Run(() => CompleteTask(task));

        private void CompleteTask(TaskItem task)
        {
            if (task != null && task.CanDoTask(_io))
            {
                task.DoTask(_io.CurrentTime.GetCurrentTime(), _tree);
                ItemPicker.TreeEditor.Cursor--;
            }
        }

        internal void Edit() => ItemPicker.Edit();
    }
}