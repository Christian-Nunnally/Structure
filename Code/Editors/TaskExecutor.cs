using System;

namespace Structure
{
    public class TaskExecutor : TaskPicker
    {
        protected readonly PersistedTree<TaskItem> _tree;
        protected Action<TaskItem> TaskCompletedAction = x => { };

        public TaskExecutor(
            string prompt,
            PersistedTree<TaskItem> tree)
            : base(prompt, "Complete", true, false, false, tree)
        {
            _tree = tree;
            PickedAction = CompleteTask;
        }

        private void CompleteTask(TaskItem task)
        {
            task.CompletedDate = CurrentTime.GetCurrentTime();
            _tree.Remove(task.ID);
            Data.Points++;
            Data.XP++;
            TaskCompletedAction(task);
        }
    }
}