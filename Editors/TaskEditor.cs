using Structure.IO;
using Structure.IO.Persistence;
using Structure.Structure;
using Structure.TaskItems;
using System;
using System.Diagnostics.Contracts;

namespace Structure.Editors
{
    public class TaskEditor : TaskExecutor
    {
        public const string InsertTaskPrompt = "Insert task";
        public const string TitlePrompt = "Task tree";
        private readonly TaskExecutor _taskExecutor;
        private readonly NodeTreeCollection<TaskItem> _tree;

        public TaskEditor(StructureIO io, StructureData data) : base(io, TitlePrompt, data?.ActiveTaskTree)
        {
            Contract.Requires(io != null);
            Contract.Requires(data != null);
            _taskExecutor = new TaskExecutor(io, TitlePrompt, data?.ActiveTaskTree);
            _taskExecutor.ItemPicker.TreeEditor.EnableDefaultInsertFunctionality(InsertTaskPrompt, _taskExecutor.ItemPicker.TreeEditor.DefaultNodeFactory);
            AddCustomAction(new UserAction("Toggle show children", () => _taskExecutor.ItemPicker.TreeEditor.ShowChildren = !_taskExecutor.ItemPicker.TreeEditor.ShowChildren, ConsoleKey.V));
            AddCustomAction(new UserAction("Copy current task", CopyCurrentTask, ConsoleKey.C));
            _tree = data?.ActiveTaskTree;
        }

        private void CopyCurrentTask()
        {
            if (_taskExecutor.ItemPicker.TreeEditor.TryGetSelectedTask(out var selectedTask))
            {
                CopyTask(selectedTask, selectedTask.ParentID);
            }
        }

        private void CopyTask(TaskItem task, string parentID)
        {
            var newTask = new TaskItem
            {
                Name = task.Name,
                Rank = task.Rank + 1,
                ParentID = parentID,
            };
            _tree.Set(newTask);
            var children = _taskExecutor.ItemPicker.TreeEditor.GetChildren(task.ID);
            children.All(x => CopyTask(x, newTask.ID));
        }

        public void Edit() => _taskExecutor.Edit();
    }
}