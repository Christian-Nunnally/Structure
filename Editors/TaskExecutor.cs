namespace Structure
{
    public class TaskExecutor : TaskPicker
    {
        protected readonly PersistedTree<TaskItem> _tree;

        public TaskExecutor(
            string prompt,
            PersistedTree<TaskItem> tree)
            : base(prompt, "Complete", true, false, false, tree)
        {
            _tree = tree;
            PickedAction = CompleteTask;
        }

        public void CompleteTask(TaskItem task)
        {
            IO.Run(() => task?.DoTask(_tree));
        }
    }
}