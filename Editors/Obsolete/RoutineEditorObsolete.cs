using Structure.Editors.Obsolete;
using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;
using System;
using System.Collections.Generic;

namespace Structure.Editors
{
    public class RoutineEditorObsolete : TreeEditorObsolete<TaskItem>
    {
        public RoutineEditorObsolete(StructureIO io, NodeTree<TaskItem> routineTree) : base(io, "Edit routines", routineTree)
        {
            EnableDefaultInsertFunctionality("Insert routine item", DefaultNodeFactory);
            var converter = TaskItemConversions.CreateTaskItemConverter();
            ItemConverter = converter;
            //AddTaskConversionStrategies();
        }

        //private void AddTaskConversionStrategies()
        //{
        //    var conversionMap = new Dictionary<Type, Func<TaskItem, TaskItem>>
        //    {
        //        { typeof(RecordFloatTaskItem), ConvertToFloatTaskItem },
        //        { typeof(RecordIntegerTaskItem), ConvertToIntegerTaskItem },
        //        { typeof(RecordStringTaskItem), ConvertToStringTaskItem }
        //    };
        //    ItemConverter.Add(typeof(TaskItem), conversionMap);

        //    conversionMap = new Dictionary<Type, Func<TaskItem, TaskItem>>
        //    {
        //        { typeof(RecordFloatTaskItem), ConvertToFloatTaskItem },
        //        { typeof(TaskItem), ConvertToTaskItem },
        //        { typeof(RecordStringTaskItem), ConvertToStringTaskItem }
        //    };
        //    ItemConverter.Add(typeof(RecordIntegerTaskItem), conversionMap);

        //    conversionMap = new Dictionary<Type, Func<TaskItem, TaskItem>>
        //    {
        //        { typeof(TaskItem), ConvertToTaskItem },
        //        { typeof(RecordIntegerTaskItem), ConvertToIntegerTaskItem },
        //        { typeof(RecordStringTaskItem), ConvertToStringTaskItem }
        //    };
        //    ItemConverter.Add(typeof(RecordFloatTaskItem), conversionMap);

        //    conversionMap = new Dictionary<Type, Func<TaskItem, TaskItem>>
        //    {
        //        { typeof(RecordFloatTaskItem), ConvertToFloatTaskItem },
        //        { typeof(RecordIntegerTaskItem), ConvertToIntegerTaskItem },
        //        { typeof(TaskItem), ConvertToTaskItem }
        //    };
        //    ItemConverter.Add(typeof(RecordStringTaskItem), conversionMap);
        //}

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