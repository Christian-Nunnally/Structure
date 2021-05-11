namespace Structure
{
    internal class TaskEditor : TaskExecutor
    {
        public TaskEditor() : base("Task tree", Data.ActiveTaskTree)
        {
            EnableDefaultInsertFunctionality("Insert task");
            CustomActions.Add(("o", TaskEditorOptions));
            CustomActions.Add(("g", () => new Beeper().Beep()));
            CustomActions.Add(("v", () => ShowChildren = !ShowChildren));
        }

        private void TaskEditorOptions()
        {
        }
    }
}