using System;
using System.Collections.Generic;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class TaskModule : Module
    {
        public Queue<Action> QueuedTasks = new Queue<Action>();

        public override int RequiredLevel => 0;

        public override void Disable()
        {
            Hotkeys.Remove(ConsoleKey.T);
            Hotkeys.Remove(ConsoleKey.N);
            QueuedTasks.Clear();
        }

        public override void Enable()
        {
            Hotkeys.Add(ConsoleKey.T, (StartNewTask, "Start task"));
            Hotkeys.Add(ConsoleKey.N, (DoQueuedTask, "Next action"));
            foreach (var task in ActiveTaskList)
            {
                QueuedTasks.Enqueue(() => DoTask(task));
            }
        }

        private void StartNewTask()
        {
            Write("Enter task: ");
            ReadLine(AddAndDoTask);
        }

        private void AddAndDoTask(string task)
        {
            ActiveTaskList.Add(task);
            DoTask(task);
        }

        private void DoTask(string task)
        {
            Clear();
            Write(task);
            PromptOptions("Task complete?",
                true,
                ('y', "yes", () => Run(() => CompleteTask(task), 1)),
                ('d', "delete", () => ActiveTaskList.Remove(task)),
                ('p', "postpone", () => QueuedTasks.Enqueue(() => DoTask(task))));
        }

        private void CompleteTask(string task)
        {
            ActiveTaskList.Remove(task);
            CompletedTaskList.Add(task);
            XP++;
            Points++;
        }

        private void DoQueuedTask()
        {
            if (QueuedTasks.Count == 0) return;
            if (QueuedTasks.TryDequeue(out var action) && action != null) Run(action);
            else DoQueuedTask();
        }
    }
}