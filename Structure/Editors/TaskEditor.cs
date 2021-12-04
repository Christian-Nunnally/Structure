namespace Structure
{
    public class TaskEditor : TaskExecutor
    {
        public const string InsertTaskPrompt = "Insert task";
        public const string TitlePrompt = "Task tree";

        public TaskEditor() : base(TitlePrompt, Data.ActiveTaskTree)
        {
            EnableDefaultInsertFunctionality(InsertTaskPrompt, DefaultNodeFactory);
            CustomActions.Add(("o", TaskEditorOptions));
            CustomActions.Add(("g", () => new Beeper().Beep()));
            CustomActions.Add(("v", () => ShowChildren = !ShowChildren));
            CustomActions.Add(("c", CopyCurrentTask));
            CustomActions.Add(("n", GoToNextActiveTask));
        }

        private void CopyCurrentTask()
        {
            if (TryGetSelectedTask(out var selectedTask))
            {
                var newTask = new TaskItem
                {
                    Name = selectedTask.Name,
                    Rank = selectedTask.Rank + 1,
                    ParentID = selectedTask.ParentID,
                };
                _tree.Set(newTask);
            }
        }

        private void TaskEditorOptions()
        {
        }

        private void GoToNextActiveTask()
        {
            if (TreeTask.OpenEditors.Count > 1)
            {
                var thisEditorsIndex = TreeTask.OpenEditors.IndexOf(this);
                if (thisEditorsIndex >= 0)
                {
                    var nextEditorIndex = (thisEditorsIndex + 1) % TreeTask.OpenEditors.Count;
                    var nextEditor = TreeTask.OpenEditors[nextEditorIndex];
                    IO.Run(nextEditor.Edit);
                }
            }
        }
    }
}