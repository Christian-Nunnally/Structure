using Structure.Editors.Obsolete;
using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;

namespace Structure.Editors
{
    public class RoutineEditorObsolete : TreeEditorObsolete<TaskItem>
    {
        public RoutineEditorObsolete(StructureIO io, NodeTree<TaskItem> routineTree) : base(io, "Edit routines", routineTree)
        {
            EnableDefaultInsertFunctionality("Insert routine item", DefaultNodeFactory);
            var converter = TaskItemConversions.CreateTaskItemConverter();
            ItemConverter = converter;
        }
    }
}