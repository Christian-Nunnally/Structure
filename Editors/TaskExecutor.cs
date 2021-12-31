namespace Structure
{
    public class TaskExecutor : TaskPicker
    {
        private readonly StructureIO _io;

        public TaskExecutor(
            StructureIO io,
            string prompt,
            NodeTreeCollection<TaskItem> tree)
            : base(io, prompt, "Complete", true, false, false, tree)
        {
            SetPickAction(CompleteTask);
            _io = io;
        }

        public virtual void CompleteTask(TaskItem task)
        {
            _io.Run(() => task?.DoTask(_io, Tree));
        }
    }
}