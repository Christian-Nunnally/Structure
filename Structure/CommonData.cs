using Structure.IO.Persistence;
using Structure.Modules.Interface;
using Structure.TaskItems;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Structure.Structure
{
    public class StructureData
    {
        public NodeTreeCollection<TaskItem> ActiveTaskTree { get; } = new NodeTreeCollection<TaskItem>();

        public ObservableCollection<TaskItem> CompletedTasks { get; } = new ObservableCollection<TaskItem>();

        public List<IModule> EnabledModules { get; } = new List<IModule>();

        public NodeTreeCollection<TaskItem> Routines { get; } = new NodeTreeCollection<TaskItem>();

        public int XP { get; set; }

        public int Points { get; set; }

        public int Level { get; set; }
    }
}