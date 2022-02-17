using Structure.Editors;
using System.Diagnostics.Contracts;

namespace Structure.TaskItems
{
    public static class TaskItemConversions
    {
        public static void AddTaskConversionStrategies(TreeEditor<TaskItem> tree)
        {
            Contract.Requires(tree != null);
            var task = typeof(TaskItem);
            var floatTask = typeof(RecordFloatTaskItem);
            var integerTask = typeof(RecordIntegerTaskItem);
            var stringTask = typeof(RecordStringTaskItem);

            tree.AddToItemConversionMap(task, floatTask, ConvertToFloatTaskItem);
            tree.AddToItemConversionMap(task, integerTask, ConvertToIntegerTaskItem);
            tree.AddToItemConversionMap(task, stringTask, ConvertToStringTaskItem);

            tree.AddToItemConversionMap(integerTask, floatTask, ConvertToFloatTaskItem);
            tree.AddToItemConversionMap(integerTask, task, ConvertToTaskItem);
            tree.AddToItemConversionMap(integerTask, stringTask, ConvertToStringTaskItem);

            tree.AddToItemConversionMap(floatTask, task, ConvertToTaskItem);
            tree.AddToItemConversionMap(floatTask, integerTask, ConvertToIntegerTaskItem);
            tree.AddToItemConversionMap(floatTask, stringTask, ConvertToStringTaskItem);

            tree.AddToItemConversionMap(stringTask, floatTask, ConvertToFloatTaskItem);
            tree.AddToItemConversionMap(stringTask, integerTask, ConvertToIntegerTaskItem);
            tree.AddToItemConversionMap(stringTask, task, ConvertToTaskItem);
        }

        public static TaskItem ConvertToTaskItem(TaskItem item)
        {
            var newItem = new TaskItem();
            item?.CopyTo(newItem);
            return newItem;
        }

        public static TaskItem ConvertToIntegerTaskItem(TaskItem item)
        {
            var newItem = new RecordIntegerTaskItem();
            item?.CopyTo(newItem);
            return newItem;
        }

        public static TaskItem ConvertToFloatTaskItem(TaskItem item)
        {
            var newItem = new RecordFloatTaskItem();
            item?.CopyTo(newItem);
            return newItem;
        }

        public static TaskItem ConvertToStringTaskItem(TaskItem item)
        {
            var newItem = new RecordStringTaskItem();
            item?.CopyTo(newItem);
            return newItem;
        }
    }
}
