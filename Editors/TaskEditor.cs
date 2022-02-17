using Structure.IO;
using Structure.IO.Persistence;
using Structure.Structure;
using Structure.TaskItems;
using System;
using System.Diagnostics.Contracts;

namespace Structure.Editors
{
    public class TaskEditor
    {
        public const string InsertTaskPrompt = "Insert task";
        public const string TitlePrompt = "Task tree";
        private readonly NodeTreeCollection<TaskItem> _tree;
        
        public TaskExecutor TaskExecutor { get; }

        public TaskEditor(StructureIO io, StructureData data)
        {
            Contract.Requires(io != null);
            Contract.Requires(data != null);
            TaskExecutor = new TaskExecutor(io, TitlePrompt, data?.ActiveTaskTree);
            TaskExecutor.ItemPicker.TreeEditor.EnableDefaultInsertFunctionality(InsertTaskPrompt, TreeEditor<TaskItem>.DefaultNodeFactory);
            TaskExecutor.AddCustomAction(new UserAction("Toggle show children", () => TaskExecutor.ItemPicker.TreeEditor.ShowChildren = !TaskExecutor.ItemPicker.TreeEditor.ShowChildren, ConsoleKey.V));
            TaskExecutor.AddCustomAction(new UserAction("Copy current task", CopyCurrentTask, ConsoleKey.C));
            _tree = data?.ActiveTaskTree;
        }

        private void CopyCurrentTask()
        {
            if (TaskExecutor.ItemPicker.TreeEditor.TryGetSelectedTask(out var selectedTask))
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
            var children = TaskExecutor.ItemPicker.TreeEditor.GetChildren(task.ID);
            children.All(x => CopyTask(x, newTask.ID));
        }

        public void Edit() => TaskExecutor.Edit();
    }
}