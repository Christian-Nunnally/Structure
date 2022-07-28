using Structure.IO;
using Structure.Program;
using Structure.Program.Utilities;
using Structure.TaskItems;
using System;

namespace Structure.Editors.Obsolete
{
    public class TaskEditorObsolete : TaskExecutorObsolete
    {
        public const string InsertTaskPrompt = "Insert task";
        public const string TitlePrompt = "Task tree";

        public TaskEditorObsolete(StructureIO io, StructureData data) : base(io, TitlePrompt, data?.Tasks)
        {
            EnableDefaultInsertFunctionality(InsertTaskPrompt, DefaultNodeFactory);
            CustomActions.Add(new UserAction("o", () => { }, ConsoleKey.O));
            CustomActions.Add(new UserAction("v", () => ShowChildren = !ShowChildren, ConsoleKey.V));
            CustomActions.Add(new UserAction("c", CopyCurrentTask, ConsoleKey.C));
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