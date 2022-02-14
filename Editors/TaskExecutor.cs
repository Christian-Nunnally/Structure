using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;

namespace Structure.Editors
{
    public class TaskExecutor : ItemPicker<TaskItem>
    {
        private readonly StructureIO _io;

        public TaskExecutor(
            StructureIO io,
            string prompt,
            NodeTreeCollection<TaskItem> tree)
            : base(io, prompt, true, false, false, tree)
        {
            SetPickAction(CompleteTask);
            _io = io;
        }

        public virtual void CompleteTask(TaskItem task)
        {
            _io.Run(() => 
            { 
                if (task != null && task.CanDoTask(_io))
                {
                    task?.DoTask(_io.CurrentTime.GetCurrentTime(), Tree); 
                }
            });
        }
    }
}