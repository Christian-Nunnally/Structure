using Structur.Modules.Interfaces;
using Structur.TaskItems;
using System.Collections.Generic;
using System.Linq;

namespace Structur.Modules.SubModules
{
    public class ActiveTaskCountCollector : StructureModule, ISubModule
    {
        public IList<TaskItem> TaskCountOverTime { get; } = new List<TaskItem>();

        protected override void OnDisable()
        {
        }

        protected override void OnEnable()
        {
            Data.Tasks.CountChanged += ActiveTaskTreeCountChanged;
        }

        private void ActiveTaskTreeCountChanged()
        {
            var dataPoint = new RecordIntegerTaskItem
            {
                CompletedDate = IO.CurrentTime.Time,
                RecordedInteger = Data.Tasks.Count(),
            };
            TaskCountOverTime.Add(dataPoint);
        }
    }
}
