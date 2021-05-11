using System;
using System.Linq;
using static Structure.IO;

namespace Structure
{
    internal class Routiner : Module
    {
        public static PersistedTree<TaskItem> Routines = new PersistedTree<TaskItem>("Routines");
        public static PersistedTree<TaskItem> ActiveRoutines = new PersistedTree<TaskItem>("ActiveRoutines");
        private static readonly PersistedInt _routinePoints = new PersistedInt("RoutinePoints");
        private UserAction _action;
        public static int RoutinePoints { get => _routinePoints.Get(); set => _routinePoints.Set(value); }

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.R, _action);
        }

        public override void Enable()
        {
            _action = Hotkey.Add(ConsoleKey.R, new UserAction("Routines", PromptRoutinerOptions));
        }

        private void DoRoutine() => DoRoutine(null);

        private void DoRoutine(TaskItem routine)
        {
            Run(() => new RoutineExecutor(routine).Edit());
        }

        private void PromptRoutinerOptions()
        {
            var start = new UserAction("Start routine", PickRoutine);
            var edit = new UserAction("Edit routines", EditRoutines);
            var resume = new UserAction("Resume routines", DoRoutine);
            var options = ActiveRoutines.Any() ? new[] { start, edit, resume } : new[] { start, edit };
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
            var children = Routines.Where(x => x.Value.ParentID == task.ID);
            var routine = new TaskItem { Task = task.Task };
            ActiveRoutines.Set(routine);
            foreach (var child in children.OrderBy(x => x.Value.Rank))
            {
                var routineTask = new TaskItem { Task = child.Value.Task, ParentID = routine.ID, Rank = child.Value.Rank };
                ActiveRoutines.Set(routineTask);
            }
            DoRoutine(routine);
        }
    }
}