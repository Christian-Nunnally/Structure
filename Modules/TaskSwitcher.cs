using Structure.Editors;
using Structure.IO;
using Structure.Modules.Interface;
using Structure.TaskItems;
using System;
using System.Linq;

namespace Structure.Modules
{
    public class TaskSwitcher : StructureModule
    {
        private UserAction _pickAction;
        private UserAction _editAction;

        protected override void OnDisable()
        {
            //Hotkey.Remove(ConsoleKey.S, _pickAction);
            //Hotkey.Remove(ConsoleKey.E, _editAction);
        }

        protected override void OnEnable()
        {
            //_pickAction = new UserAction(DoRoutineActionDescription, PickRoutine);
            //_editAction = new UserAction(EditRoutineActionDescription, EditRoutines);
            //Hotkey.Add(ConsoleKey.R, _pickAction);
            //Hotkey.Add(ConsoleKey.E, _editAction);
        }
    }
}