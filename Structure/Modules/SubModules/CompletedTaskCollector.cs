using Structur.Modules.Interfaces;
using Structur.TaskItems;
using System.Collections.Generic;

namespace Structur.Modules.SubModules
{
    public class CompletedTaskCollector : StructureModule, ISubModule
    {
        public IList<TaskItem> CompletedTasks { get; } = new List<TaskItem>();

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
