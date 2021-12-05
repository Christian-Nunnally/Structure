using Structure.Code.Modules;
using System;
using System.Linq;
using static Structure.IO;

namespace Structure
{
    public class RoutinerV2 : Module, IObsoleteModule
    {
        private UserAction _pickAction;
        private UserAction _editAction;

        public IModule UpgradedModule => new RoutinerV3();

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.R, _pickAction);
            Hotkey.Remove(ConsoleKey.E, _editAction);
        }

        protected override void OnEnable()
        {
            _pickAction = Hotkey.Add(ConsoleKey.R, new UserAction("Do routine", PickRoutine));
            _editAction = Hotkey.Add(ConsoleKey.E, new UserAction("Edit routines", EditRoutines));
        }

        private static TaskItem CopyRoutineToTaskList(TaskItem task, string parentId = null)
        {
            var copy = new TaskItem { Name = task.Name, Rank = task.Rank, ParentID = parentId };
            CommonData.ActiveTaskTree.Set(copy);
            var children = CommonData.Routines.Where(x => x.Value.ParentID == task.ID);
            foreach (var child in children.OrderBy(x => x.Value.Rank))
            {
                CopyRoutineToTaskList(child.Value, copy.ID);
            }
            return copy;
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
            var task = CopyRoutineToTaskList(routine);
            DoRoutine(task);
        }
    }
}