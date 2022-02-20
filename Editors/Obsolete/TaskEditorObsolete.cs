using Structure.IO;
using Structure.Structure;
using Structure.TaskItems;
using System;
using System.Diagnostics.Contracts;

namespace Structure.Editors.Obsolete
{
    public class TaskEditorObsolete : TaskExecutorObsolete
    {
        public const string InsertTaskPrompt = "Insert task";
        public const string TitlePrompt = "Task tree";
        private readonly StructureData _data;
        private readonly StructureIO _io;

        public TaskEditorObsolete(StructureIO io, StructureData data) : base(io, TitlePrompt, data?.Tasks)
        {
            Contract.Requires(io != null);
            Contract.Requires(data != null);
            EnableDefaultInsertFunctionality(InsertTaskPrompt, DefaultNodeFactory);
            CustomActions.Add(new UserAction("o", () => { }, ConsoleKey.O));
            CustomActions.Add(new UserAction("v", () => ShowChildren = !ShowChildren, ConsoleKey.V));
            CustomActions.Add(new UserAction("c", CopyCurrentTask, ConsoleKey.C));
            _data = data;
            _io = io;
        }

        private void CopyCurrentTask()
        {
            if (TryGetSelectedTask(out var selectedTask))
            {
                var newTask = new TaskItem
                {
                    Name = selectedTask.Name,
                    Rank = selectedTask.Rank + 1,
                    ParentID = selectedTask.ParentID,
                };
                Tree.Set(newTask);
            }
        }
    }
}