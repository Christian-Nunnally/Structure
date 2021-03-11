using System;

namespace Structure
{
    public class TaskExecutor : TaskPicker
    {
        protected Action<TaskItem> TaskCompletedAction = x => { };
        private readonly PersistedTree<TaskItem> _tree;
        private readonly PersistedTree<TaskItem> _completedTaskTree;

        public TaskExecutor(
            string prompt,
            PersistedTree<TaskItem> tree,
            PersistedTree<TaskItem> completedTaskTree)
            : base(prompt, "Complete", true, false, false, tree)
        {
            _tree = tree;
            _completedTaskTree = completedTaskTree;
            PickedAction = CompleteTask;
        }

        private void CompleteTask(TaskItem task)
        {
            task.CompletedDate = DateTime.Now;
            _tree.Remove(task.ID);
            _completedTaskTree.Set(task);
            Data.Points++;
            Data.XP++;
            TaskCompletedAction(task);
        }
    }
}