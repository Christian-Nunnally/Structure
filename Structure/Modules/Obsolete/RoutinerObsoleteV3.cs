using Structur.Editors;
using Structur.Editors.Obsolete;
using Structur.IO;
using Structur.Modules.Interfaces;
using Structur.TaskItems;
using System;
using System.Linq;

namespace Structur.Modules.Obsolete
{
    public class RoutinerObsoleteV3 : StructureModule, IObsoleteModule
    {
        private UserAction _pickAction;
        private UserAction _editAction;

        public IModule UpgradeModule() => new Routiner();

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.R, _pickAction);
            Hotkey.Remove(ConsoleKey.E, _editAction);
        }

        protected override void OnEnable()
        {
            _pickAction = new UserAction("Do routine", PickRoutine);
            _editAction = new UserAction("Edit routines", EditRoutines);
            Hotkey.Add(ConsoleKey.R, _pickAction);
            Hotkey.Add(ConsoleKey.E, _editAction);
        }

        private TaskItem CopyRoutineToTaskList(TaskItem task, string parentId = null)
        {
            var copy = (TaskItem)task.Copy();
            copy.ParentID = parentId;
            Data.Tasks.Set(copy);
            var children = Data.Routines.Where(x => x.Value.ParentID == task.ID);
            foreach (var child in children.OrderBy(x => x.Value.Rank))
            {
                CopyRoutineToTaskList(child.Value, copy.ID);
            }
            return copy;
        }

        private void DoRoutine(TaskItem routine)
        {
            IO.Run(() =>
            {
                var editor = new TaskEditorObsolete(IO, Data);
                editor.SetParent(routine);
                editor.Edit();
            });
        }

        private void EditRoutines()
        {
            IoC.Get<INewsPrinter>().EnqueueNews("Editing");
            IO.Run(() => new RoutineEditorObsolete(IO, Data.Routines).Edit());
        }

        private void PickRoutine()
        {
            IO.Run(() => new TaskPickerObsolete(IO, "Pick routine to start", "Start", false, true, true, Data.Routines, StartRoutine).Edit());
        }

        private void StartRoutine(TaskItem routine)
        {
            var task = CopyRoutineToTaskList(routine);
            DoRoutine(task);
        }
    }
}