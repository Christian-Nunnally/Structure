using Structure.Code.Modules;
using System;
using System.Linq;
using static Structure.IO;

namespace Structure
{
    internal class Routiner : Module, IObsoleteModule
    {
        private UserAction _action;

        public IModule UpgradedModule => new RoutinerV2();

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.R, _action);
        }

        protected override void OnEnable()
        {
            _action = Hotkey.Add(ConsoleKey.R, new UserAction("Routines", PromptRoutinerOptions));
        }

        private static void CopyRoutineToTaskList(TaskItem task, string parentId = null)
        {
            var copy = task.Copy();
            CommonData.ActiveTaskTree.Set(copy);
            var children = CommonData.Routines.Where(x => x.Value.ParentID == task.ID);
            foreach (var child in children.OrderBy(x => x.Value.Rank))
            {
                CopyRoutineToTaskList(child.Value, copy.ID);
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
            Run(() => new RoutineEditor(CommonData.Routines).Edit());
        }

        private void PickRoutine()
        {
            Run(() => new TaskPicker("Pick routine to start", "Start", false, true, true, CommonData.Routines, StartRoutine).Edit());
        }

        private void StartRoutine(TaskItem routine)
        {
            CopyRoutineToTaskList(routine);
            DoRoutine(routine);
        }
    }
}