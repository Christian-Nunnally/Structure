using Structur.Editors;
using Structur.Graphing;
using Structur.IO;
using Structur.IO.Input;
using Structur.IO.Output;
using Structur.Modules.Interfaces;
using Structur.Modules.SubModules;
using Structur.TaskItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Structur.IO.Persistence;
using Structur.Program;
using Structur.Program.Utilities;

namespace Structur.Modules
{
    public class DataGrapher : StructureModule
    {
        private const string ModuleHotkeyDescription = "Data Grapher";
        private const string SetYAxisModeActionDescription = "Set y axis mode";
        private const string CompletedTasksDataSetDescription = "Completed Tasks";
        private const string ActiveTaskCountDataSetDescription = "Active Task Count";
        private UserAction _startAction;
        private string _searchTerm;
        private bool _listValues;
        private bool _exit;
        private TaskHistoryQuery _selectedQuery;
        private NodeTree<TaskItem> _dataRoutines;
        private readonly List<TaskHistoryQuery> _queries = new();
        private bool _dataSetsInitialized;
        private bool _isUsingRealData;
        private readonly List<(string Name, IList<TaskItem> Data)> _dataSets = new();
        private readonly NodeTree<TaskHistoryQuery> _queriesTree = new();

        protected override void OnDisable()
        {
            Hotkey.Remove(_startAction);
        }

        protected override void OnEnable()
        {
            
            _startAction = new UserAction(ModuleHotkeyDescription, Start, ConsoleKey.H);
            Hotkey.Add(_startAction);
        }

        private void PopulateDataSets()
        {
            _dataSets.Clear();
            var activeTaskCountCollector = new ActiveTaskCountCollector();
            var completedTaskCollector = new CompletedTaskCollector();
            if (!IO.SkipUnescesscaryOperations)
            {
                RunStructureWithModules(activeTaskCountCollector, completedTaskCollector);
                _isUsingRealData = true;
            }
            _dataSetsInitialized = true;
            _dataSets.Add((ActiveTaskCountDataSetDescription, activeTaskCountCollector.TaskCountOverTime));
            _dataSets.Add((CompletedTasksDataSetDescription, completedTaskCollector.CompletedTasks));

            _queries.All(x => x.DataSet = _dataSets.First(d => d.Name == x.DataSet.Name));
            if (_selectedQuery == null) AddQuery();
        }

        private void RunStructureWithModules(params IModule[] modules)
        {
            var ioc = new StructureIoC();
            ioc.Register<Hotkey>();
            ioc.Register<CurrentTime>();
            ioc.Register<StructureData>();
            var news = new NoOpNewsPrinter();
            ioc.Register<IBackgroundProcess>(() => news);
            ioc.Register<INewsPrinter>(() => news);
            var localIO = new StructureIO(ioc);
            var startingModules = StartingModules.Create().ToList();
            modules.All(x => x.Enable(ioc, localIO));
            startingModules.AddRange(modules);
            var program = new StructureProgram(ioc, localIO, startingModules.ToArray());
            localIO.ProgramInput = new ExitingStructureInput(program);
            localIO.ProgramOutput = new NoOpOutput();
            var thread = new Thread(() => program.Run(new ExitToken()));
            IO.Run(() =>
            {
                IO.Write("Loading data sets...");
                thread.Start();
                while (localIO.ProgramInput.IsInputAvailable()) { Thread.Sleep(100); }
            });
            _dataRoutines = ioc.Get<StructureData>().Routines;
        }

        private void Start()
        {
            while(!_exit)
            {
                if ((!_isUsingRealData && !IO.SkipUnescesscaryOperations) || !_dataSetsInitialized) PopulateDataSets();
                ShowHistoryAndListOptions(BasicFilter);
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
            IO.ClearBuffer();
            PrintTitle();
            ShowData(filter);
            ListChartOptions();
            IO.ClearStaleOutput();
        }

        private void PrintTitle()
        {
            IO.Write("Task History");
            IO.Write();
            int i = 0;
            ModifySelectedQueries(x => IO.Write(QueryToPrettyString(x, i++)));
            IO.Write();
        }

        private static string QueryToPrettyString(TaskHistoryQuery x, int index)
        {
            return $"Query {index} - Data: {x.DataSet.Name} - Mode: {x.AggregationMode.Method.Name} - Range: {x.AggregationRange}\n          Grouping: {x.AggregationRange} - Interpolate: {x.InterpolateEmptyRanges} - Filter: {x.SearchTerm}";
        }

        private void ShowData(Predicate<TaskItem> filter)
        {
            var selectedQueriesValues = new List<IList<(string, double)>>();
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

        private void PrintValuesAsList(IList<IList<(string Label, double Value)>> listOfValues)
        {
            var i = 1;
            foreach (var values in listOfValues)
            {
                IO.Write($"Data from query #{i++}:");
                values.All(x => IO.Write($"{x.Label} : {x.Value}"));
            }
        }

        private void GraphValues(IList<IList<(string Label, double Value)>> listOfValues)
        {
            var consoleGraph = new ConsoleGraph(80, 20);
            consoleGraph.Print(IO, listOfValues);
        }

        private void ListChartOptions()
        {
            var options = new[]
            {
                new UserAction("Modify how values are computed", ChangeHowValuesAreComputed, ConsoleKey.V),
                new UserAction("Modify what is being graphed", ChangeWhatIsBeingGraphed, ConsoleKey.M),
                new UserAction("Add/remove/select query", AddOrRemoveQueries, ConsoleKey.A),
                new UserAction("Change axis settings", ChangeAxisSettings, ConsoleKey.C),
                new UserAction("List raw values", ToggleListValues, ConsoleKey.L),
                new UserAction("Exit", Exit, ConsoleKey.Escape),
            };
            IO.ReadOptions("Task history options", null, options);
        }

        private void ChangeAxisSettings()
        {
            var options = new[]
{
                new UserAction("Change x axis range", ChangeRange, ConsoleKey.X),
                new UserAction("Exit", Exit, ConsoleKey.Escape),
            };
            IO.ReadOptions("Task history options", null, options);
        }

        private void AddOrRemoveQueries()
        {
            var options = new[]
{
                new UserAction("Select query", SelectQuery, ConsoleKey.S),
                new UserAction("Add query", AddQuery, ConsoleKey.A),
                new UserAction("Exit", Exit, ConsoleKey.Escape),
            };
            IO.ReadOptions("Change what is bing graphed", null, options);
        }

        private void ChangeWhatIsBeingGraphed()
        {
            var options = new[]
            {
                new UserAction("Add tasks copied from another task", AddCopiedTasks, ConsoleKey.A),
                new UserAction("Select data set", SelectDataSet, ConsoleKey.S),
                new UserAction("Filter by word", SetSearchTerm, ConsoleKey.F),
                new UserAction("Clear filters", ClearFilters, ConsoleKey.C),
                new UserAction("Exit", Exit, ConsoleKey.Escape),
            };
            IO.ReadOptions("Change what is bing graphed", null, options);
        }

        private void ChangeHowValuesAreComputed()
        {
            var options = new[]
{
                new UserAction("Change grouping", ChangeGrouping, ConsoleKey.G),
                new UserAction("Y axis", ChangeYAxisMode, ConsoleKey.Y),
                new UserAction("Toggle interpolate zeros", ToggleInterpolateZeros, ConsoleKey.T),
                new UserAction("Exit", Exit, ConsoleKey.Escape),
            };
            IO.ReadOptions("Change how values are computed", null, options);
        }

        private void AddQuery()
        {
            _selectedQuery = new TaskHistoryQuery { DataSet = _dataSets.FirstOrDefault(), SearchTerm = _searchTerm };
            _queries.Add(_selectedQuery);
        }

        private void SelectQuery()
        {
            ItemPicker<TaskHistoryQuery> picker = new(IO, "Select a query", true, true, _queriesTree, true, SelectQuery);
            IO.Run(picker.Edit);
        }

        private void SelectQuery(TaskHistoryQuery query)
        {
            _selectedQuery = query;
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
            var options = new NodeTree<TaskItem>();
            foreach (var dataSet in _dataSets)
            {
                options.Set(dataSet.Name, new TaskItem() { Name = dataSet.Name });
            }

            ItemPicker<TaskItem> picker = new(IO, "Select data set", true, true, options, true, SelectDataSet);
            IO.Run(picker.Edit);
        }

        private void SelectDataSet(TaskItem dataSetRepresentitive)
        {
            var dataSet = _dataSets.FirstOrDefault(x => x.Name == dataSetRepresentitive.Name);
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
            var taskPicker = new ItemPicker<TaskItem>(IO, "Pick task to include copies of", true, true, _dataRoutines, false, AddTasksCopiedFrom);
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
                new UserAction("Item count", SetToCountMode, ConsoleKey.C),
                new UserAction("Max value", SetToMaxValueMode, ConsoleKey.M),
                new UserAction("Min value", SetToMinValueMode, ConsoleKey.N),
                new UserAction("Sum of values", SetToSumValueMode, ConsoleKey.S),
                new UserAction("Average value", SetToMeanValueMode, ConsoleKey.A)
            };

            IO.ReadOptions(SetYAxisModeActionDescription, null, options);
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
            var range = TimeSpan.MinValue;
            ModifySelectedQueries(x => range = x.Range);
            IO.ReadTimeSpan("Set new range", SetRange, range);
        }

        private void SetRange(TimeSpan range)
        {
            if (range > TimeSpan.MinValue) ModifySelectedQueries(x => x.Range = range);
        }

        private void ChangeGrouping()
        {
            var grouping = TimeSpan.MinValue;
            ModifySelectedQueries(x => grouping = x.Range);
            IO.ReadTimeSpan("Set new grouping", SetGrouping, grouping);
        }


        private void SetGrouping(TimeSpan grouping)
        {
            if (grouping > TimeSpan.MinValue) ModifySelectedQueries(x => x.AggregationRange = grouping);
        }

        private void ModifySelectedQueries(Action<TaskHistoryQuery> modify)
        {
            modify(_selectedQuery);
            _queriesTree.Get
        }

/*
 * 
 * 
 * 
 * 
 * 
 * 
 */
    }
}
