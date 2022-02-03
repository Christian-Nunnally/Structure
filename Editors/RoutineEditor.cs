using System;
using System.Collections.Generic;

namespace Structure
{
    public class RoutineEditor : TreeEditorObsolete<TaskItem>
    {
        public RoutineEditor(StructureIO io, NodeTreeCollection<TaskItem> routineTree) : base(io, "Edit routines", routineTree)
        {
            EnableDefaultInsertFunctionality("Insert routine item", DefaultNodeFactory);
            AddTaskConversionStrategies();
        }

        private void AddTaskConversionStrategies()
        {
            var conversionMap = new Dictionary<Type, Func<TaskItem, TaskItem>>
            {
                { typeof(RecordFloatTaskItem), ConvertToFloatTaskItem },
                { typeof(RecordIntegerTaskItem), ConvertToIntegerTaskItem },
                { typeof(RecordStringTaskItem), ConvertToStringTaskItem }
            };
            ItemConversionMap.Add(typeof(TaskItem), conversionMap);

            conversionMap = new Dictionary<Type, Func<TaskItem, TaskItem>>
            {
                { typeof(RecordFloatTaskItem), ConvertToFloatTaskItem },
                { typeof(TaskItem), ConvertToTaskItem },
                { typeof(RecordStringTaskItem), ConvertToStringTaskItem }
            };
            ItemConversionMap.Add(typeof(RecordIntegerTaskItem), conversionMap);

            conversionMap = new Dictionary<Type, Func<TaskItem, TaskItem>>
            {
                { typeof(TaskItem), ConvertToTaskItem },
                { typeof(RecordIntegerTaskItem), ConvertToIntegerTaskItem },
                { typeof(RecordStringTaskItem), ConvertToStringTaskItem }
            };
            ItemConversionMap.Add(typeof(RecordFloatTaskItem), conversionMap);

            conversionMap = new Dictionary<Type, Func<TaskItem, TaskItem>>
            {
                { typeof(RecordFloatTaskItem), ConvertToFloatTaskItem },
                { typeof(RecordIntegerTaskItem), ConvertToIntegerTaskItem },
                { typeof(TaskItem), ConvertToTaskItem }
            };
            ItemConversionMap.Add(typeof(RecordStringTaskItem), conversionMap);
        }

        private TaskItem ConvertToTaskItem(TaskItem item)
        {
            var newItem = new TaskItem();
            item.CopyTo(newItem);
            return newItem;
        }

        private TaskItem ConvertToIntegerTaskItem(TaskItem item)
        {
            var newItem = new RecordIntegerTaskItem();
            item.CopyTo(newItem);
            return newItem;
        }

        private TaskItem ConvertToFloatTaskItem(TaskItem item)
        {
            var newItem = new RecordFloatTaskItem();
            item.CopyTo(newItem);
            return newItem;
        }

        private TaskItem ConvertToStringTaskItem(TaskItem item)
        {
            var newItem = new RecordStringTaskItem();
            item.CopyTo(newItem);
            return newItem;
        }
    }
}