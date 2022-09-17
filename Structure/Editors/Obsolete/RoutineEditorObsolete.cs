using Structur.Editors.Obsolete;
using Structur.IO;
using Structur.IO.Persistence;
using Structur.TaskItems;

namespace Structur.Editors
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