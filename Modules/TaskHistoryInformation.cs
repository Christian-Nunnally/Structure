using Structure.Graphing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Modules
{
    public class TaskHistoryInformation : Module
    {
        private UserAction _startAction;
        private int _range = 30;
        private int _grouping = 1;
        private Func<List<TaskItem>, double> _aggregationMode = CountAggregationFunction;
        private TaskItem _routineParent;

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
        }

        private void Start()
        {
            ShowHistory(x => true);
        }

        private void ShowHistory(Predicate<TaskItem> filter)
        {
            var tasks = CurrentData.CompletedTasks.Where(x => x.CompletedDate + new TimeSpan(_range, 0, 0, 0, 0) > DateTime.Now && filter(x)).ToList();
            
            if (_routineParent is object)
            {
                tasks = tasks.Where(t => t.CopiedFromID == _routineParent.ID).ToList();
            }

            var values = new List<(string, double)>();
            var totalCount = values.Count;

            for (int i = 1; i < _range; i += _grouping)
            {
                var groupedTasks = tasks.Where(x => x.CompletedDate + new TimeSpan(i, 0, 0, 0, 0) > DateTime.Now).ToList();
                foreach (var task in groupedTasks)
                {
                    tasks.Remove(task);
                }
                values.Insert(0, ($"-{i} days", _aggregationMode(groupedTasks)));
            }

            var consoleGraph = new ConsoleGraph(80, 20);
            consoleGraph.Print(IO, values);

            var changeRangeOption = new UserAction("Change range", ChangeRange);
            var changeGroupingOption = new UserAction("Change grouping", ChangeGrouping);
            var setRoutineParent = new UserAction("Set routine parent", SetRoutineParent);
            var changeYAxisOption = new UserAction("Y axis", ChangeYAxisMode);
            var listRawValues = new UserAction("List raw values", () => ListValues(values));

            IO.PromptOptions(
                "Task history options", 
                false, changeRangeOption, 
                changeGroupingOption,
                setRoutineParent,
                changeYAxisOption, 
                listRawValues);
        }

        private void SetRoutineParent()
        {
            IO.Run(() => new TaskPicker(IO, "Pick routine parent", "Select", true, true, true, CurrentData.Routines, SetRoutineParentToTask).Edit());
            IO.Run(Start);
        }

        private void SetRoutineParentToTask(TaskItem routineParent)
        {
            _routineParent = routineParent;
        }

        private void ListValues(List<(string Label, double Value)> values)
        {
            IO.Write();
            foreach (var (Label, Value) in values)
            {
                IO.Write($"{Label} : {Value}");
            }
            IO.ReadAny();
            IO.Run(Start);
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
            IO.Run(Start);
        }

        private void SetToMaxValueMode()
        {
            _aggregationMode = MaxAggregationFunction;
            IO.Run(Start);
        }

        private void SetToCountMode()
        {
            _aggregationMode = CountAggregationFunction;
            IO.Run(Start);
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
            IO.Run(Start);
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
            IO.Run(Start);
        }
    }
}
