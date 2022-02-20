using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;

namespace Structure.Editors.Obsolete
{
    public class TaskExecutorObsolete : TaskPickerObsolete
    {
        private readonly StructureIO _io;

        public TaskExecutorObsolete(
            StructureIO io,
            string prompt,
            NodeTree<TaskItem> tree)
            : base(io, prompt, "Complete", true, false, false, tree)
        {
            SetPickAction(CompleteTask);
            _io = io;
        }

        public virtual void CompleteTask(TaskItem task)
        {
            _io.Run(() =>
            {
                if (task != null && task is RecordFloatTaskItem || task is RecordIntegerTaskItem || task is RecordStringTaskItem)
                {
                    if (task.CanDoTask(_io)) task.DoTask(_io.CurrentTime.GetCurrentTime(), Tree);
                }
                else
                {
                    task.DoTask(_io.CurrentTime.GetCurrentTime(), Tree);
                }
            });
        }
    }
}