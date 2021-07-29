namespace Structure
{
    public class TaskEditor : TaskExecutor
    {
        public const string InsertTaskPrompt = "Insert task";
        public const string TitlePrompt = "Task tree";

        public TaskEditor() : base(TitlePrompt, Data.ActiveTaskTree)
        {
            EnableDefaultInsertFunctionality(InsertTaskPrompt);
            CustomActions.Add(("o", TaskEditorOptions));
            CustomActions.Add(("g", () => new Beeper().Beep()));
            CustomActions.Add(("v", () => ShowChildren = !ShowChildren));
            CustomActions.Add(("c", CopyCurrentTask));
            CustomActions.Add(("n", GoToNextActiveTask));
        }

        private void CopyCurrentTask()
        {
            var children = GetChildren(_currentParent);
            if (children.Count > 0 && children.Count > _cursor)
            {
                var taskToCopy = children[_cursor];
                var newTask = new TaskItem
                {
                    Task = taskToCopy.Task,
                    Rank = taskToCopy.Rank + 1,
                    ParentID = taskToCopy.ParentID,
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