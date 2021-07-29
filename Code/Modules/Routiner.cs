using System;
using System.Linq;
using static Structure.IO;

namespace Structure
{
    internal class Routiner : Module
    {
        public static PersistedTree<TaskItem> Routines = new PersistedTree<TaskItem>("Routines");
        private UserAction _action;

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.R, _action);
        }

        public override void Enable()
        {
            _action = Hotkey.Add(ConsoleKey.R, new UserAction("Routines", PromptRoutinerOptions));
        }

        private static void CopyChildren(TaskItem task, string parentId = null)
        {
            var copy = new TaskItem { Task = task.Task, Rank = task.Rank, ParentID = parentId };
            Data.ActiveTaskTree.Set(copy);
            var children = Routines.Where(x => x.Value.ParentID == task.ID);
            foreach (var child in children.OrderBy(x => x.Value.Rank))
            {
                CopyChildren(child.Value, copy.ID);
            }
        }

        private void DoRoutine(TaskItem routine)
        {
            Run(() =>
            {
                var editor = new TaskEditor();
                editor.SetParent(routine);
                editor.Edit();
            });
        }

        private void PromptRoutinerOptions()
        {
            var start = new UserAction("Start routine", PickRoutine);
            var edit = new UserAction("Edit routines", EditRoutines);
            var options = new[] { start, edit };
            PromptOptions("Routines", false, options);
        }

        private void EditRoutines()
        {
            Run(() => new RoutineEditor(Routines).Edit());
        }

        private void PickRoutine()
        {
            Run(() => new TaskPicker("Pick routine to start", "Start", false, true, true, Routines, StartRoutine).Edit());
        }

        private void StartRoutine(TaskItem task)
        {
            var routine = new TaskItem { Task = task.Task };
            Data.ActiveTaskTree.Set(routine);
            CopyChildren(routine);
            DoRoutine(routine);
        }
    }
}