using Structure.Editors;
using Structure.IO;
using Structure.Modules.Interface;
using Structure.TaskItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Modules
{
    public class TaskSwitcher : StructureModule
    {
        private readonly Queue<TaskItem> _activeTasksQueue = new Queue<TaskItem>();
        private const string SwitchToNextActiveTaskPrompt = "Switch to next active task";
        private const string ActivateTaskPrompt = "Activate a task";
        private UserAction _switchToActiveTask;
        private UserAction _activateTask;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.S, _switchToActiveTask);
            Hotkey.Remove(ConsoleKey.N, _activateTask);
        }

        protected override void OnEnable()
        {
            _switchToActiveTask = new UserAction(SwitchToNextActiveTaskPrompt, SwitchToNextActiveTask);
            _activateTask = new UserAction(ActivateTaskPrompt, ActivateTask);
            Hotkey.Add(ConsoleKey.S, _switchToActiveTask);
            Hotkey.Add(ConsoleKey.N, _activateTask);
            //Hotkey.Add(ConsoleKey.M, _deactivateTask);
        }

        private void ActivateTask()
        {
            var picker = new ItemPicker<TaskItem>(IO, "Pick task to activate", true, true, Data.Tasks, true, ActivateTask);
            IO.Run(picker.Edit);
        }

        private void ActivateTask(TaskItem task)
        {
            _activeTasksQueue.Enqueue(task);
        }

        private void SwitchToNextActiveTask()
        {
            if (!_activeTasksQueue.Any()) return;
            var picker = new TaskExecutor(IO, "Task tree", Data.Tasks, true);
            var currentTask = _activeTasksQueue.Dequeue();
            if (Data.Tasks.Get(currentTask.ID) != currentTask)
            {
                SwitchToNextActiveTask();
                return;
            }
            _activeTasksQueue.Enqueue(currentTask);
            picker.ItemPicker.TreeEditor.SetParent(currentTask);
            IO.Run(picker.Edit);
        }
    }
}