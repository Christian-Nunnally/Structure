using Structure.Graphing;
using Structure.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Modules
{
    public class TaskHistoryInformation : StructureModule
    {
        private const string ModuleHotkeyDescription = "Task history";
        private const string SetYAxisModeActionDescription = "Set y axis mode";
        private const string CompletedTasksDataSetDescription = "Completed Tasks";
        private const string ActiveTaskCountDataSetDescription = "Active Task Count";
        private UserAction _startAction;
        private string _searchTerm = null;
        private bool _listValues;
        private bool _exit;
        private TaskHistoryQuery _selectedQuery;
        private bool _selectAllQueries = true;
        private readonly List<TaskHistoryQuery> _queries = new List<TaskHistoryQuery>();

        public List<(string Name, IList<TaskItem> Data)> DataSets { get; private set; } = new List<(string Name, IList<TaskItem> Data)>();

        public TaskHistoryInformation()
        {
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.H, _startAction);
        }

        protected override void OnEnable()
        {
            _startAction = new UserAction(ModuleHotkeyDescription, Start);
            Hotkey.Add(ConsoleKey.H, _startAction);

            if (DataSets.Count == 0)
            {
                DataSets.Add((CompletedTasksDataSetDescription, Data.CompletedTasks));
                DataSets.Add((ActiveTaskCountDataSetDescription, Data.TaskCountOverTime));

                AddQuery();
            }
        }

        private void Start()
        {
            while(!_exit)
            {
                IO.Run(() => ShowHistoryAndListOptions(BasicFilter));
            }
            _exit = false;
        }

        private bool BasicFilter(TaskItem task)
        {
            if (_searchTerm != null)
            {
                return task.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        private void ShowHistoryAndListOptions(Predicate<TaskItem> filter)
        {
            PrintTitle();
            ShowData(filter);
            ListChartOptions();
        }

        private void PrintTitle()
        {
            IO.Write(_selectedQuery.DataSet.Name);
        }

        private void ShowData(Predicate<TaskItem> filter)
        {
            var selectedQueriesValues = new List<List<(string, double)>>();
            ModifySelectedQueries(x => selectedQueriesValues.Add(x.ComputeValues(filter)));

            if (!selectedQueriesValues.Any()) return;
            if (_listValues)
            {
                PrintValuesAsList(selectedQueriesValues);
            }
            else
            {
                GraphValues(selectedQueriesValues);
            }
        }

        private void PrintValuesAsList(List<List<(string Label, double Value)>> listOfValues)
        {
            var i = 1;
            foreach (var values in listOfValues)
            {
                IO.Write($"Data from query #{i++}:");
                values.All(x => IO.Write($"{x.Label} : {x.Value}"));
            }
        }

        private void GraphValues(List<List<(string Label, double Value)>> listOfValues)
        {
            var consoleGraph = new ConsoleGraph(80, 20);
            consoleGraph.Print(IO, listOfValues);
        }

        private void ListChartOptions()
        {
            var options = new[]
            {
                new UserAction("Change range", ChangeRange),
                new UserAction("Change grouping", ChangeGrouping),
                new UserAction("Add tasks copied from another task", AddCopiedTasks),
                new UserAction("Y axis", ChangeYAxisMode),
                new UserAction("List raw values", ToggleListValues),
                new UserAction("Toggle interpolate zeros", ToggleInterpolateZeros),
                new UserAction("Select data set", SelectDataSet),
                new UserAction("Filter by word", SetSearchTerm),
                new UserAction("Clear filters", ClearFilters),
                new UserAction("Select query", SelectQuery),
                new UserAction("Add query", AddQuery),
                new UserAction("Exit", Exit, ConsoleKey.Escape),
            };

            IO.PromptOptions("Task history options", false, options);
        }

        private void AddQuery()
        {
            _selectedQuery = new TaskHistoryQuery { DataSet = DataSets.First(), SearchTerm = _searchTerm };
            _queries.Add(_selectedQuery);
        }

        private void SelectQuery()
        {
            var options = new List<UserAction>();
            int i = 0;
            foreach (var query in _queries)
            {
                options.Add(new UserAction($"{i}", () => SelectQuery(query)));
            }
            options.Add(new UserAction("All", SelectAllQueries));
            IO.Run(() => IO.PromptOptions("Select a query or all", false, options.ToArray()));
        }

        private void SelectQuery(TaskHistoryQuery query)
        {
            _selectedQuery = query;
            _selectAllQueries = false;
        }

        private void SelectAllQueries()
        {
            _selectAllQueries = true;
        }

        private void SetSearchTerm()
        {
            IO.Read(x => _searchTerm = x, KeyGroups.AlphanumericKeysPlus, new[] { ConsoleKey.Enter, ConsoleKey.LeftArrow });
        }

        private void ClearFilters()
        {
            ModifySelectedQueries(x => { x.CopiedFromIds.Clear(); });
            _searchTerm = null;
        }

        private void Exit()
        {
            _exit = true;
        }

        private void SelectDataSet()
        {
            var options = new List<UserAction>();
            foreach (var dataSet in DataSets)
            {
                options.Add(new UserAction(dataSet.Name, () => SelectDataSet(dataSet)));
            }
            IO.PromptOptions("Select data set", false, "", options.ToArray());
        }

        private void SelectDataSet((string Name, IList<TaskItem> Data) dataSet)
        {
            ModifySelectedQueries(x => x.DataSet = dataSet);
        }

        private void ToggleInterpolateZeros()
        {
            bool currentValue = false;
            ModifySelectedQueries(x => currentValue = x.InterpolateEmptyRanges);
            ModifySelectedQueries(x => x.InterpolateEmptyRanges = !currentValue);
        }

        private void AddCopiedTasks()
        {
            var taskPicker = new TaskPicker(IO, "Pick task to include copies of", true, true, true, Data.Routines, AddTasksCopiedFrom);
            IO.Run(taskPicker.Edit);
        }

        private void AddTasksCopiedFrom(TaskItem task)
        {
            ModifySelectedQueries(x => x.CopiedFromIds.Add(task));
        }

        private void ToggleListValues()
        {
            _listValues = !_listValues;
        }

        private void ChangeYAxisMode()
        {
            var options = new[]
            {
                new UserAction("Item count", SetToCountMode),
                new UserAction("Max value", SetToMaxValueMode),
                new UserAction("Min value", SetToMinValueMode),
                new UserAction("Sum of values", SetToSumValueMode),
                new UserAction("Average value", SetToMeanValueMode)
            };

            IO.PromptOptions(SetYAxisModeActionDescription, false, options);
        }

        private void SetToSumValueMode()
        {
            ModifySelectedQueries(x => x.AggregationMode = AggregationFunctions.SumAggregationFunction);
        }

        private void SetToMaxValueMode()
        {
            ModifySelectedQueries(x => x.AggregationMode = AggregationFunctions.MaxAggregationFunction);
        }

        private void SetToCountMode()
        {
            ModifySelectedQueries(x => x.AggregationMode = AggregationFunctions.CountAggregationFunction);
        }

        private void SetToMinValueMode()
        {
            ModifySelectedQueries(x => x.AggregationMode = AggregationFunctions.MinAggregationFunction);
        }

        private void SetToMeanValueMode()
        {
            ModifySelectedQueries(x => x.AggregationMode = AggregationFunctions.MeanAggregationFunction);
        }

        private void ChangeRange()
        {
            IO.ReadInteger("Set new range", SetRange);
        }

        private void SetRange(int range)
        {
            if (range > 0) ModifySelectedQueries(x => x.Range = new TimeSpan(range, 0, 0, 0));
        }

        private void ChangeGrouping()
        {
            IO.ReadInteger("Set new grouping", SetGrouping);
        }

        private void SetGrouping(int grouping)
        {
            if (grouping > 0) ModifySelectedQueries(x => x.AggregationRange = new TimeSpan(grouping, 0, 0, 0));
        }

        private void ModifySelectedQueries(Action<TaskHistoryQuery> modify)
        {
            if (_selectAllQueries) _queries.All(modify);
            else modify(_selectedQuery);
        }
    }
}
