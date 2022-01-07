﻿using System;
using System.Linq;

namespace Structure
{
    public class RoutinerV4 : StructureModule
    {
        private UserAction _pickAction;
        private UserAction _editAction;

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

        private TaskItem CopyRoutineToTaskList(TaskItem task, string parentId = null)
        {
            var copy = task.Copy();
            copy.ParentID = parentId;
            Data.ActiveTaskTree.Set(copy);
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
                var editor = new TaskEditor(IO, Data);
                editor.SetParent(routine);
                editor.Edit();
            });
        }

        private void EditRoutines()
        {
            IO.News("Editing");
            IO.Run(() => new RoutineEditor(IO, Data.Routines).Edit());
        }

        private void PickRoutine()
        {
            IO.Run(() => new TaskPicker(IO, "Pick routine to start", "Start", true, true, true, Data.Routines, StartRoutine).Edit());
        }

        private void StartRoutine(TaskItem routine)
        {
            var task = CopyRoutineToTaskList(routine);
            DoRoutine(task);
        }
    }
}