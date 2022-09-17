using Structur.Editors;
using Structur.IO;
using Structur.Modules.Interfaces;
using System;

namespace Structur.Modules
{
    public class TaskTree : StructureModule
    {
        public const string Title = "Task tree";
        public const string RunTaskTreePrompt = "Open task tree";

        private UserAction _doTasks;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.T, _doTasks);
        }

        protected override void OnEnable()
        {
            _doTasks = new UserAction(RunTaskTreePrompt, Start);
            Hotkey.Add(ConsoleKey.T, _doTasks);
        }
        
        private void Start()
        {
            var editor = new TaskExecutor(IO, Title, Data.Tasks, true);
            IO.Run(editor.Edit);
        }
    }
}