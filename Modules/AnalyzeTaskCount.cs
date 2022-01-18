using System.Collections.Specialized;
using System.Linq;

namespace Structure.Modules
{
    public class AnalyzeTaskCount : StructureModule
    {
        protected override void OnDisable()
        {
        }

        protected override void OnEnable()
        {
            // TODO: Listen to active task tree count change instead.
            Data.CompletedTasks.CollectionChanged += CompletedTasksChanged;
        }

        private void CompletedTasksChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var dataPoint = new RecordIntegerTaskItem
                {
                    CompletedDate = IO.CurrentTime.GetCurrentTime(),
                    RecordedInteger = Data.ActiveTaskTree.Count(),
                };
                Data.TaskCountOverTime.Add(dataPoint);
            }
        }
    }
}
