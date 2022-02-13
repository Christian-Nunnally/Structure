using Structure.IO;
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
        private readonly StructureData _data;

        public TaskEditor(StructureIO io, StructureData data) : base(io, TitlePrompt, data?.ActiveTaskTree)
        {
            Contract.Requires(io != null);
            Contract.Requires(data != null);
            EnableDefaultInsertFunctionality(InsertTaskPrompt, DefaultNodeFactory);
            CustomActions.Add(new UserAction("Toggle show children", () => ShowChildren = !ShowChildren, ConsoleKey.V));
            CustomActions.Add(new UserAction("Copy current task", CopyCurrentTask, ConsoleKey.C));
            _data = data;
        }

        private void CopyCurrentTask()
        {
            if (TryGetSelectedTask(out var selectedTask))
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
            Tree.Set(newTask);
            var children = GetChildren(task.ID);
            children.All(x => CopyTask(x, newTask.ID));
        }

        public override void CompleteTask(TaskItem task)
        {
            base.CompleteTask(task);
            _data.CompletedTasks.Add(task);
        }
    }
}