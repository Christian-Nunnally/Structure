using Structure.Editors.Obsolete;
using Structure.IO.Persistence;
using Structure.TaskItems;
using System.Collections.Generic;

namespace Structure.Structure
{
    public class StructureData
    {
        public NodeTreeCollection<TaskItem> ActiveTaskTree { get; } = new NodeTreeCollection<TaskItem>();

        public NodeTreeCollection<TaskItem> Routines { get; } = new NodeTreeCollection<TaskItem>();

        // TODO: Remove.
        public List<TaskEditorObsolete> OpenEditors { get; } = new List<TaskEditorObsolete>();
    }
}