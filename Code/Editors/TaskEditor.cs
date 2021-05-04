using System;

namespace Structure
{
    internal class TaskEditor : TaskExecutor
    {
        public TaskEditor() : base("Task tree", Data.ActiveTaskTree, Data.CompletedTaskTree)
        {
            CustomActions.Add(("i", PromptToInsertTask));
            CustomActions.Add(("o", TaskEditorOptions));
            CustomActions.Add(("g", () => new Beeper().Beep()));
            CustomActions.Add(("v", () => ShowChildren = !ShowChildren));
            NoChildrenAction = PromptToInsertTask;
        }

        private void TaskEditorOptions()
        {
            throw new NotImplementedException();
        }

        private void PromptToInsertTask()
        {
            var index = NumberOfVisibleTasks;
            IO.WriteNoLine($"\nInsert task #{index}: ");
            IO.Read(s => AddTask(s, _currentParent, index), ConsoleKey.Enter, ConsoleKey.LeftArrow);
            if (NumberOfVisibleTasks == 0) ViewParent();
        }

        private void AddTask(string description, string parentID, int rank)
        {
            if (string.IsNullOrEmpty(description))
            {
                return;
            }
            var task = new TaskItem
            {
                ParentID = parentID,
                Task = description,
                Rank = rank
            };
            Data.ActiveTaskTree.Set(task.ID, task);
        }
    }
}