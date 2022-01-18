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
        private TaskItem _routineParent;
        private bool _interpolateZeros;
        private bool _listValues;
        private (string Name, IList<TaskItem> Data) _currentDataSet;
        private bool _exit;

        public List<(string Name, IList<TaskItem> Data)> DataSets { get; private set; } = new List<(string Name, IList<TaskItem> Data)>();

        private static double CountAggregationFunction(List<TaskItem> list) => list.Count;

        private static double MaxAggregationFunction(List<TaskItem> list)
        {
            double currentMax = 0;
            foreach (var item in list)
            {
                if (item is RecordFloatTaskItem floatTaskItem)
                {
                    if (floatTaskItem.RecordedFloat > currentMax)
                    {
                        currentMax = floatTaskItem.RecordedFloat;
                    }
                }
                else if (item is RecordIntegerTaskItem integerTaskItem)
                {
                    if (integerTaskItem.RecordedInteger > currentMax)
                    {
                        currentMax = integerTaskItem.RecordedInteger;
                    }
                }
                else
                {
                    if (1 > currentMax)
                    {
                        currentMax = 1;
                    }
                }
            }
            return currentMax;
        }

        private static double SumAggregationFunction(List<TaskItem> list)
        {
            double currentSum = 0;
            foreach (var item in list)
            {
                if (item is RecordFloatTaskItem floatTaskItem)
                {
                    currentSum += floatTaskItem.RecordedFloat;
                }
                else if (item is RecordIntegerTaskItem integerTaskItem)
                {
                    currentSum += integerTaskItem.RecordedInteger;
                }
            }
            return currentSum;
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
                IO.Run(() => ShowHistoryAndListOptions(x => true));
            }
            _exit = false;
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
            if (_routineParent is object)
            {
                tasks = tasks.Where(t => t.CopiedFromID == _routineParent.ID).ToList();
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
            var setRoutineParent = new UserAction("Set routine parent", SetRoutineParent);
            var changeYAxisOption = new UserAction("Y axis", ChangeYAxisMode);
            var listRawValues = new UserAction("List raw values", ToggleListValues);
            var toggleInterpolateZeros = new UserAction("Toggle interpolate zeros", ToggleInterpolateZeros);
            var selectDataSet = new UserAction("Select data set", SelectDataSet);
            var exit = new UserAction("Exit", Exit, ConsoleKey.Escape);

            IO.PromptOptions(
                "Task history options",
                false, changeRangeOption,
                changeGroupingOption,
                setRoutineParent,
                changeYAxisOption,
                listRawValues,
                toggleInterpolateZeros,
                selectDataSet,
                exit);
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
            IO.PromptOptions("Select data set", false, options.ToArray());
        }

        private void SelectDataSet((string Name, IList<TaskItem> Data) dataSet)
        {
            _currentDataSet = dataSet;
        }

        private void ToggleInterpolateZeros()
        {
            _interpolateZeros = !_interpolateZeros;
        }

        private void SetRoutineParent()
        {
            IO.Run(() => new TaskPicker(IO, "Pick routine parent", "Select", true, true, true, Data.Routines, SetRoutineParentToTask).Edit());
        }

        private void SetRoutineParentToTask(TaskItem routineParent)
        {
            _routineParent = routineParent;
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
            var setToCountMode = new UserAction("Count mode", SetToCountMode);
            var setToMaxValueMode = new UserAction("Max value", SetToMaxValueMode);
            var setToSumMode = new UserAction("Sum value", SetToSumValueMode);

            IO.PromptOptions(
                "Set y axis mode", 
                false, 
                setToCountMode, 
                setToMaxValueMode,
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
