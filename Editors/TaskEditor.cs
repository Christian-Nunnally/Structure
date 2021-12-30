namespace Structure
{
    public class TaskEditor : TaskExecutor
    {
        public const string InsertTaskPrompt = "Insert task";
        public const string TitlePrompt = "Task tree";
        private readonly CommonData _data;
        private readonly StructureIO _io;

        public TaskEditor(StructureIO io, CommonData data) : base(io, TitlePrompt, data.ActiveTaskTree)
        {
            EnableDefaultInsertFunctionality(InsertTaskPrompt, DefaultNodeFactory);
            CustomActions.Add(("o", TaskEditorOptions));
            CustomActions.Add(("v", () => ShowChildren = !ShowChildren));
            CustomActions.Add(("c", CopyCurrentTask));

            // TODO: Can remove?
            CustomActions.Add(("n", GoToNextActiveTask));
            _data = data;
            _io = io;
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
                    _io.Run(nextEditor.Edit);
                }
            }
        }

        public override void CompleteTask(TaskItem task)
        {
            base.CompleteTask(task);
            _data.CompletedTasks.Add(task);
        }
    }
}