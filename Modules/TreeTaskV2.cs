using Structure.Editors;
using Structure.IO;
using Structure.Modules.Interface;
using System;

namespace Structure.Modules
{
    internal class TreeTaskV2 : StructureModule
    {
        private UserAction _doTasks;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.T, _doTasks);
        }

        protected override void OnEnable()
        {
            _doTasks = new UserAction("Do tasks", Start);
            Hotkey.Add(ConsoleKey.T, _doTasks);
        }
        
        private void Start()
        {
            var editor = new TaskExecutor(IO, "Task tree", Data.Tasks, true);
            IO.Run(editor.Edit);
        }
    }
}