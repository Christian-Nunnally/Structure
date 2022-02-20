using Structure.Modules.Interface;
using Structure.TaskItems;
using System.Collections.Generic;

namespace Structure.Modules.SubModules
{
    public class CompletedTaskCollector : StructureModule, ISubModule
    {
        public List<TaskItem> CompletedTasks { get; } = new List<TaskItem>();

        protected override void OnDisable()
        {
        }

        protected override void OnEnable()
        {
            Data.Tasks.NodeRemoved += NodeRemoved;
        }

        private void NodeRemoved(TaskItem removedNode)
        {
            if (removedNode.CompletedDate != default)
            {
                CompletedTasks.Add(removedNode);
            }
        }
    }
}
