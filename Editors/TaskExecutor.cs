using System.Collections.Generic;

namespace Structure
{
    public class TaskExecutor : TaskPicker
    {
        protected readonly PersistedTree<TaskItem> _tree;
        public static IReadOnlyList<TaskItem> CompletedTasks => _completedTasks;

        private static readonly List<TaskItem> _completedTasks = new List<TaskItem>();

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
            _completedTasks.Add(task);
        }
    }
}