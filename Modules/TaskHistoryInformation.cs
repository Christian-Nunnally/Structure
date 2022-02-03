using Structure.Graphing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Modules
{
    public class TaskHistoryInformation : StructureModule
    {
        private UserAction _startAction;
        private int _range = 30;
        private int _grouping = 1;
        private Func<List<TaskItem>, double> _aggregationMode = CountAggregationFunction;
        private readonly List<TaskItem> _copiedFrom = new List<TaskItem>();
        private string _searchTerm = null;
        private bool _interpolateZeros;
        private bool _listValues;
        private (string Name, IList<TaskItem> Data) _currentDataSet;
        private bool _exit;

        public List<(string Name, IList<TaskItem> Data)> DataSets { get; private set; } = new List<(string Name, IList<TaskItem> Data)>();

        private static double CountAggregationFunction(List<TaskItem> list) => list.Count;

        private static double MaxAggregationFunction(List<TaskItem> list)
        {
            if (list.Count == 0) return 0;
            return list.Max(x => GetNumericValueOfItem(x));
        }

        private static double SumAggregationFunction(List<TaskItem> list)
        {
            if (list.Count == 0) return 0;
            return list.Sum(item => GetNumericValueOfItem(item));
        }

        private static double MinAggregationFunction(List<TaskItem> list)
        {
            if (list.Count == 0) return 0;
            return list.Min(item => GetNumericValueOfItem(item));
        }

        private static double MeanAggregationFunction(List<TaskItem> list)
        {
            if (list.Count == 0) return 0;
            return SumAggregationFunction(list) / list.Count;
        }

        private static double GetNumericValueOfItem(TaskItem item)
        {
            if (item is RecordFloatTaskItem floatTaskItem)
            {
                return floatTaskItem.RecordedFloat;
            }
            else if (item is RecordIntegerTaskItem integerTaskItem)
            {
                return integerTaskItem.RecordedInteger;
            }
            return 0;
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.H, _startAction);
        }

        protected override void OnEnable()
        {
            _startAction = Hotkey.Add(ConsoleKey.H, new UserAction("Task history", Start));

            if (DataSets.Count == 0)
            {
                DataSets.Add(("Completed Tasks", Data.CompletedTasks));
                DataSets.Add(("Active Task Count", Data.TaskCountOverTime));
                _currentDataSet = DataSets.First();
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
            IO.Write(_currentDataSet.Name);
        }

        private void ShowData(Predicate<TaskItem> filter)
        {
            var values = ComputeValues(filter);
            if (_listValues)
            {
                ListValues(values);
            }
            else
            {
                GraphValues(values);
            }
        }

        public TaskHistoryInformation()
        {
        }

        private void GraphValues(List<(string Label, double Value)> values)
        {
            var consoleGraph = new ConsoleGraph(80, 20);
            consoleGraph.Print(IO, values);
        }

        private List<(string Label, double Value)> ComputeValues(Predicate<TaskItem> filter)
        {
            var tasks = _currentDataSet.Data.Where(x => x.CompletedDate + new TimeSpan(_range, 0, 0, 0, 0) > DateTime.Now && filter(x)).ToList();
            if (_copiedFrom.Count > 0)
            {
                tasks = tasks.Where(t => _copiedFrom.Any(x => x.ID == t.CopiedFromID)).ToList();
            }
            var values = new List<(string Label, double Value)>();
            for (int i = 1; i < _range; i += _grouping)
            {
                var groupedTasks = tasks.Where(x => x.CompletedDate + new TimeSpan(i, 0, 0, 0, 0) > DateTime.Now).ToList();
                foreach (var task in groupedTasks)
                {
                    tasks.Remove(task);
                }
                if (groupedTasks.Count == 0 && _interpolateZeros)
                {
                    values.Insert(0, ($"", 0));
                }
                else
                {
                    values.Insert(0, ($"-{i} days", _aggregationMode(groupedTasks)));
                }
            }

            if (_interpolateZeros) values = InterpolateEmptyValues(values);
            return values;
        }

        private static List<(string Label, double Value)> InterpolateEmptyValues(List<(string Label, double Value)> values)
        {
            var interpolatedValues = new List<(string Label, double Value)>();
            for (int i = 0; i < values.Count; i++)
            {
                if (string.IsNullOrEmpty(values[i].Label))
                {
                    var min = 0.0;
                    var indexOf = -1;
                    var max = 0.0;
                    var maxI = -1;

                    for (int j = i; j >= 0; j--)
                    {
                        if (!IsValueEmpty(values[j]))
                        {
                            min = values[j].Value;
                            indexOf = j;
                            break;
                        }
                    }

                    for (int j = i; j < values.Count; j++)
                    {
                        if (!IsValueEmpty(values[j]))
                        {
                            max = values[j].Value;
                            maxI = j;
                            break;
                        }
                    }
                    if (indexOf == -1)
                    {
                        min = max;
                        indexOf = i;
                    }
                    if (maxI == -1)
                    {
                        max = min;
                        maxI = i;
                    }

                    var dist1 = i - indexOf;
                    var dist2 = maxI - i;
                    var value = (max * dist1 / (dist1 + dist2)) + (min * dist2 / (dist1 + dist2));
                    interpolatedValues.Add((values[i].Label, value));
                }
                else
                {
                    interpolatedValues.Add((values[i].Label, values[i].Value));
                }
            }
            return interpolatedValues;
        }

        private static bool IsValueEmpty((string Label, double Value) value) => string.IsNullOrEmpty(value.Label);

        private void ListChartOptions()
        {
            var changeRangeOption = new UserAction("Change range", ChangeRange);
            var changeGroupingOption = new UserAction("Change grouping", ChangeGrouping);
            var addCopiedTasks = new UserAction("Add tasks copied from another task", AddCopiedTasks);
            var changeYAxisOption = new UserAction("Y axis", ChangeYAxisMode);
            var listRawValues = new UserAction("List raw values", ToggleListValues);
            var toggleInterpolateZeros = new UserAction("Toggle interpolate zeros", ToggleInterpolateZeros);
            var selectDataSet = new UserAction("Select data set", SelectDataSet);
            var setSearchTerm = new UserAction("Filter by word", SetSearchTerm);
            var clearFilters = new UserAction("Clear filters", ClearFilters);
            var exit = new UserAction("Exit", Exit, ConsoleKey.Escape);

            IO.PromptOptions(
                "Task history options",
                false,
                 "",
                changeRangeOption, 
                changeGroupingOption,
                addCopiedTasks,
                changeYAxisOption,
                listRawValues,
                toggleInterpolateZeros,
                selectDataSet,
                setSearchTerm,
                clearFilters,
                exit);
        }

        private void SetSearchTerm()
        {
            IO.Read(x => _searchTerm = x, KeyGroups.AlphanumericKeysPlus, new[] { ConsoleKey.Enter, ConsoleKey.LeftArrow });
        }

        private void ClearFilters()
        {
            _copiedFrom.Clear();
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
            _currentDataSet = dataSet;
        }

        private void ToggleInterpolateZeros()
        {
            _interpolateZeros = !_interpolateZeros;
        }

        private void AddCopiedTasks()
        {
            IO.Run(() => new TaskPickerObsolete(IO, "Pick routine parent", "Select", true, true, true, Data.Routines, AddTasksCopiedFrom).Edit());
        }

        private void AddTasksCopiedFrom(TaskItem task)
        {
            _copiedFrom.Add(task);
        }

        private void ToggleListValues()
        {
            _listValues = !_listValues;
        }

        private void ListValues(List<(string Label, double Value)> values)
        {
            IO.Write();
            foreach (var (Label, Value) in values)
            {
                IO.Write($"{Label} : {Value}");
            }
        }

        private void ChangeYAxisMode()
        {
            var setToCountMode = new UserAction("Item count", SetToCountMode);
            var setToMaxValueMode = new UserAction("Max value", SetToMaxValueMode);
            var setToMinValueMode = new UserAction("Min value", SetToMinValueMode);
            var setToSumMode = new UserAction("Sum of values", SetToSumValueMode);
            var setToMeanMode = new UserAction("Average value", SetToMeanValueMode);

            IO.PromptOptions(
                "Set y axis mode", 
                false,
                 "",
                setToCountMode, 
                setToMaxValueMode,
                setToMinValueMode,
                setToMeanMode,
                setToSumMode);
        }

        private void SetToSumValueMode()
        {
            _aggregationMode = SumAggregationFunction;
        }

        private void SetToMaxValueMode()
        {
            _aggregationMode = MaxAggregationFunction;
        }

        private void SetToCountMode()
        {
            _aggregationMode = CountAggregationFunction;
        }

        private void SetToMinValueMode()
        {
            _aggregationMode = MinAggregationFunction;
        }

        private void SetToMeanValueMode()
        {
            _aggregationMode = MeanAggregationFunction;
        }

        private void ChangeRange()
        {
            IO.ReadInteger("Set new range", SetRange);
        }

        private void SetRange(int range)
        {
            if (range > 0)
            {
                _range = range;
            }
        }

        private void ChangeGrouping()
        {
            IO.ReadInteger("Set new grouping", SetGrouping);
        }

        private void SetGrouping(int grouping)
        {
            if (grouping > 0)
            {
                _grouping = grouping;
            }
        }
    }
}
