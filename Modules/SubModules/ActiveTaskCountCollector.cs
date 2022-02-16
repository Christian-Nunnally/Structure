using Structure.Modules.Interface;
using Structure.TaskItems;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Modules.SubModules
{
    public class ActiveTaskCountCollector : StructureModule, ISubModule
    {
        public List<TaskItem> TaskCountOverTime { get; } = new List<TaskItem>();

        protected override void OnDisable()
        {
        }

        protected override void OnEnable()
        {
            Data.ActiveTaskTree.CountChanged += ActiveTaskTreeCountChanged;
        }

        private void ActiveTaskTreeCountChanged()
        {
            var dataPoint = new RecordIntegerTaskItem
            {
                CompletedDate = IO.CurrentTime.GetCurrentTime(),
                RecordedInteger = Data.ActiveTaskTree.Count(),
            };
            TaskCountOverTime.Add(dataPoint);
        }
    }
}
