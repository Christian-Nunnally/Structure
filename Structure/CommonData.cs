using System.Collections.Generic;

namespace Structure
{
    public class StructureData
    {
        public NodeTreeCollection<TaskItem> ActiveTaskTree { get; } = new NodeTreeCollection<TaskItem>();

        public List<TaskItem> CompletedTasks { get; } = new List<TaskItem>();

        public List<IModule> EnabledModules { get; } = new List<IModule>();

        public NodeTreeCollection<TaskItem> Routines { get; } = new NodeTreeCollection<TaskItem>();

        public int XP { get; set; }

        public int Points { get; set; }

        public int Level { get; set; }
    }
}