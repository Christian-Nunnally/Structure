using Structur.Editors;
using Structur.IO.Persistence;
using Structur.TaskItems;
using System;

namespace Structur.IO
{
    public static class StructureIOExtensions
    {
        public static void ReadInteger(this StructureIO io, string prompt, Action<int> continuation)
        {
            void continueWhenInteger(string x)
            {
                if (int.TryParse(x, out var integer)) continuation(integer);
                else
                {
                    io.Write($"'{x}' is not a valid integer.");
                    io.Run(() => io.ReadInteger(prompt, continuation));
                }
            }
            io.Write(prompt);
            io.Read(continueWhenInteger, KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
        }

        public static void ReadTimeSpan(this StructureIO io, string prompt, Action<TimeSpan> continuation, TimeSpan defaultTime)
        {
            var days = defaultTime.Days;
            var hours = defaultTime.Hours;
            var minutes = defaultTime.Minutes;
            var seconds = defaultTime.Seconds;
            var milliseconds = defaultTime.Milliseconds;
            var list = new NodeTree<TaskItem>();
            var daysTask = new RecordIntegerTaskItem { Name = $"days = {days}", RecordedInteger = days };
            list.Set(daysTask);
            var hoursTask = new RecordIntegerTaskItem { Name = $"hours = {hours}", RecordedInteger = hours };
            list.Set(hoursTask);
            var minutesTask = new RecordIntegerTaskItem { Name = $"minutes = {minutes}", RecordedInteger = minutes };
            list.Set(minutesTask);
            var secondsTask = new RecordIntegerTaskItem { Name = $"seconds = {seconds}", RecordedInteger = seconds };
            list.Set(secondsTask);
            var millisecondsTask = new RecordIntegerTaskItem { Name = $"milliseconds = {milliseconds}", RecordedInteger = milliseconds };
            list.Set(millisecondsTask);
            var treeEditor = new TaskExecutor(io, prompt, list, false);
            io.Write(prompt);

            treeEditor.Edit();
            
            var time = new TimeSpan(daysTask.RecordedInteger, hoursTask.RecordedInteger, minutesTask.RecordedInteger, secondsTask.RecordedInteger, millisecondsTask.RecordedInteger);
            continuation(time);
        }
    }
}
