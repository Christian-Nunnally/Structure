namespace Structure.TaskItems
{
    public static class TaskItemConversions
    {
        public static ItemConverter<TaskItem> CreateTaskItemConverter()
        {
            var task = typeof(TaskItem);
            var floatTask = typeof(RecordFloatTaskItem);
            var integerTask = typeof(RecordIntegerTaskItem);
            var stringTask = typeof(RecordStringTaskItem);
            var itemConverter = new ItemConverter<TaskItem>();

            itemConverter.RegisterConversion(task, floatTask, ConvertToFloatTaskItem);
            itemConverter.RegisterConversion(task, integerTask, ConvertToIntegerTaskItem);
            itemConverter.RegisterConversion(task, stringTask, ConvertToStringTaskItem);

            itemConverter.RegisterConversion(integerTask, floatTask, ConvertToFloatTaskItem);
            itemConverter.RegisterConversion(integerTask, task, ConvertToTaskItem);
            itemConverter.RegisterConversion(integerTask, stringTask, ConvertToStringTaskItem);

            itemConverter.RegisterConversion(floatTask, task, ConvertToTaskItem);
            itemConverter.RegisterConversion(floatTask, integerTask, ConvertToIntegerTaskItem);
            itemConverter.RegisterConversion(floatTask, stringTask, ConvertToStringTaskItem);

            itemConverter.RegisterConversion(stringTask, floatTask, ConvertToFloatTaskItem);
            itemConverter.RegisterConversion(stringTask, integerTask, ConvertToIntegerTaskItem);
            itemConverter.RegisterConversion(stringTask, task, ConvertToTaskItem);
            return itemConverter;
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
