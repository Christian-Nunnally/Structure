using System.Collections.Generic;

namespace Structure
{
    public class CommonData
    {
        public readonly NodeTreeCollection<TaskItem> ActiveTaskTree = new NodeTreeCollection<TaskItem>();
        public readonly List<TaskItem> CompletedTasks = new List<TaskItem>();
        public readonly List<IModule> EnabledModules = new List<IModule>();
        public readonly NodeTreeCollection<TaskItem> Routines = new NodeTreeCollection<TaskItem>();

        public int XP { get; set; }
        public int Points { get; set; }
        public int Level { get; set; }
    }
}