using Structure.IO;
using Structure.Modules.Obsolete;
using Structure.Structure;
using Structure.TaskItems;
using System;
using System.Diagnostics.Contracts;

namespace Structure.Editors.Obsolete
{
    public class TaskEditorObsolete : TaskExecutorObsolete
    {
        private static int ugh = 0;
        public const string InsertTaskPrompt = "Insert task";
        public const string TitlePrompt = "Task tree";
        private readonly StructureData _data;
        private readonly StructureIO _io;

        public TaskEditorObsolete(StructureIO io, StructureData data) : base(io, TitlePrompt, data?.ActiveTaskTree)
        {
            Contract.Requires(io != null);
            Contract.Requires(data != null);
            EnableDefaultInsertFunctionality(InsertTaskPrompt, DefaultNodeFactory);
            CustomActions.Add(new UserAction("o", TaskEditorOptions, ConsoleKey.O));
            CustomActions.Add(new UserAction("v", () => ShowChildren = !ShowChildren, ConsoleKey.V));
            CustomActions.Add(new UserAction("c", CopyCurrentTask, ConsoleKey.C));
            CustomActions.Add(new UserAction("n", GoToNextActiveTask, ConsoleKey.N));
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

        private void TaskEditorOptions()
        {
            ugh++;
            if (ugh < 5) return;
            // This has been selected 5 times already :(
        }

        private void GoToNextActiveTask()
        {
            if (TreeTask.OpenEditors.Count > 1)
            {
                var thisEditorsIndex = TreeTask.OpenEditors.IndexOf(this);
                if (thisEditorsIndex >= 0)
                {
                    var nextEditorIndex = (thisEditorsIndex + 1) % TreeTask.OpenEditors.Count;
                    var nextEditor = TreeTask.OpenEditors[nextEditorIndex];
                    nextEditor.ShouldExit = false;
                    _io.Run(nextEditor.Edit);
                }
            }
        }

        public override void CompleteTask(TaskItem task)
        {
            base.CompleteTask(task);
            _data.CompletedTasks.Add(task);
        }
    }
}