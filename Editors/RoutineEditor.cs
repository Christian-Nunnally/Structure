using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;

namespace Structure.Editors
{
    public class RoutineEditor
    {
        private readonly TreeEditor<TaskItem> _treeEditor;

        public RoutineEditor(StructureIO io, NodeTreeCollection<TaskItem> routineTree)
        {
            _treeEditor = new TreeEditor<TaskItem>(io, "Edit routines", routineTree);
            _treeEditor.EnableDefaultInsertFunctionality("Insert routine item", TreeEditor<TaskItem>.DefaultNodeFactory);
            TaskItemConversions.AddTaskConversionStrategies(_treeEditor);
        }

        public void Edit() => _treeEditor.Edit();
    }
}