using Structure.Editors;
using Structure.IO;
using Structure.IO.Persistence;
using Structure.Modules.Interface;
using Structure.TaskItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Modules
{
    public class TaskSwitcher : StructureModule
    {
        private NodeTree<TaskItem> ActiveTasks { get; } = new NodeTree<TaskItem>();
        private readonly Queue<TaskItem> _activeTasksQueue = new Queue<TaskItem>();
        private const string SwitchToNextActiveTaskPrompt = "Switch to next active task";
        private const string ActivateTaskPrompt = "Pick task to add to active task list";
        private const string DeactivateTaskPrompt = "Remove task from the active task list";
        private UserAction _switchToActiveTask;
        private UserAction _activateTask;
        private UserAction _deactivateTask;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.S, _switchToActiveTask);
            Hotkey.Remove(ConsoleKey.N, _activateTask);
            Hotkey.Remove(ConsoleKey.B, _deactivateTask);
        }

        protected override void OnEnable()
        {
            _switchToActiveTask = new UserAction(SwitchToNextActiveTaskPrompt, SwitchToNextActiveTask);
            _activateTask = new UserAction(ActivateTaskPrompt, ActivateTask);
            _deactivateTask = new UserAction(DeactivateTaskPrompt, DeactivateTask);
            Hotkey.Add(ConsoleKey.S, _switchToActiveTask);
            Hotkey.Add(ConsoleKey.N, _activateTask);
            Hotkey.Add(ConsoleKey.B, _deactivateTask);
        }

        private void ActivateTask()
        {
            var picker = new ItemPicker<TaskItem>(IO, ActivateTaskPrompt, true, true, Data.Tasks, true, ActivateTask);
            IO.Run(picker.Edit);
        }

        private void ActivateTask(TaskItem task)
        {
            _activeTasksQueue.Enqueue(task);
            ActiveTasks.Set(task);
        }

        private void DeactivateTask()
        {
            var picker = new ItemPicker<TaskItem>(IO, DeactivateTaskPrompt, true, true, ActiveTasks, true, DeactivateTask);
            IO.Run(picker.Edit);
        }

        private void DeactivateTask(TaskItem taskToDeactivate)
        {
            for (int i = 0; i < _activeTasksQueue.Count; i++)
            {
                var task = _activeTasksQueue.Dequeue();
                if (task.ID != taskToDeactivate.ID) _activeTasksQueue.Enqueue(task);
            }
            ActiveTasks.Remove(taskToDeactivate);
        }

        private void SwitchToNextActiveTask()
        {
            if (!_activeTasksQueue.Any()) return;
            var picker = new TaskExecutor(IO, "Task tree", Data.Tasks, true);
            var currentTask = _activeTasksQueue.Dequeue();
            if (Data.Tasks.Get(currentTask.ID) != currentTask)
            {
                ActiveTasks.Remove(currentTask);
                SwitchToNextActiveTask();
                return;
            }
            _activeTasksQueue.Enqueue(currentTask);
            picker.ItemPicker.TreeEditor.SetParent(currentTask);
            IO.Run(picker.Edit);
        }
    }
}