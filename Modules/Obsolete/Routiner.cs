using Structure.Code.Modules;
using System;
using System.Linq;

namespace Structure
{
    internal class Routiner : StructureModule, IObsoleteModule
    {
        private UserAction _action;

        public IModule UpgradeModule() => new RoutinerV2();

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.R, _action);
        }

        protected override void OnEnable()
        {
            _action = new UserAction("Routines", PromptRoutinerOptions);
            Hotkey.Add(ConsoleKey.R, _action);
        }

        private void CopyRoutineToTaskList(TaskItem task)
        {
            var copy = task.Copy();
            Data.ActiveTaskTree.Set(copy);
            var children = Data.Routines.Where(x => x.Value.ParentID == task.ID);
            foreach (var child in children.OrderBy(x => x.Value.Rank))
            {
                CopyRoutineToTaskList(child.Value);
            }
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

        private void PromptRoutinerOptions()
        {
            var start = new UserAction("Start routine", PickRoutine);
            var edit = new UserAction("Edit routines", EditRoutines);
            var options = new[] { start, edit };
            IO.PromptOptions("Routines", false, "", options);
        }

        private void EditRoutines()
        {
            IO.Run(() => new RoutineEditor(IO, Data.Routines).Edit());
        }

        private void PickRoutine()
        {
            IO.Run(() => new TaskPickerObsolete(IO, "Pick routine to start", "Start", false, true, true, Data.Routines, StartRoutine).Edit());
        }

        private void StartRoutine(TaskItem routine)
        {
            CopyRoutineToTaskList(routine);
            DoRoutine(routine);
        }
    }
}