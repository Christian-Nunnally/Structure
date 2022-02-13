using Structure.Editors;
using Structure.IO;
using Structure.Modules.Interface;
using Structure.TaskItems;
using System;
using System.Linq;

namespace Structure.Modules
{
    public class RoutinerV4 : StructureModule
    {
        private const string PickRoutineToStartPrompt = "Pick routine to start";
        private const string DoRoutineActionDescription = "Do routine";
        private const string EditRoutineActionDescription = "Edit routines";
        private UserAction _pickAction;
        private UserAction _editAction;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.R, _pickAction);
            Hotkey.Remove(ConsoleKey.E, _editAction);
        }

        protected override void OnEnable()
        {
            _pickAction = new UserAction(DoRoutineActionDescription, PickRoutine);
            _editAction = new UserAction(EditRoutineActionDescription, EditRoutines);
            Hotkey.Add(ConsoleKey.R, _pickAction);
            Hotkey.Add(ConsoleKey.E, _editAction);
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
            var editor = new TaskEditor(IO, Data);
            editor.SetParent(routine);
            IO.Run(editor.Edit);
        }

        private void EditRoutines()
        {
            var editor = new RoutineEditor(IO, Data.Routines);
            IO.Run(editor.Edit);
        }

        private void PickRoutine()
        {
            var picker = new TaskPicker(IO, PickRoutineToStartPrompt, true, true, true, Data.Routines, CopyRoutineToTaskListAndBegin);
            IO.Run(picker.Edit);
        }

        private void CopyRoutineToTaskListAndBegin(TaskItem routine)
        {
            var task = CopyRoutineToTaskList(routine);
            DoRoutine(task);
        }
    }
}