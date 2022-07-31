using Structur.Editors;
using Structur.IO;
using Structur.Modules.Interfaces;
using System;

namespace Structur.Modules
{
    internal class TreeTask : StructureModule
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