namespace Structure
{
    public class RoutineEditor : TreeEditor<TaskItem>
    {
        public RoutineEditor(PersistedTree<TaskItem> routineTree) : base("Edit routines", routineTree)
        {
            EnableDefaultInsertFunctionality("Insert routine item");
        }
    }
}