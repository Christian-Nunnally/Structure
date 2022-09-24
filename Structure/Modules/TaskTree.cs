using Structur.Editors;
using Structur.IO;
using Structur.Modules.Interfaces;
using System;

namespace Structur.Modules
{
    public class TaskTree : StructureModule
    {
        public static readonly ConsoleKeyInfo OpenTaskTreeHotkey = new('t', ConsoleKey.T, shift: false, alt: false, control: true);

        public const string Title = "Task tree";
        public const string RunTaskTreePrompt = "Open task tree";

        private UserAction _doTasks;

        protected override void OnDisable()
        {
            Hotkey.Remove(OpenTaskTreeHotkey.Key, _doTasks);
        }

        protected override void OnEnable()
        {
            _doTasks = new UserAction(RunTaskTreePrompt, Start);
            Hotkey.Add(OpenTaskTreeHotkey.Key, _doTasks);
        }
        
        private void Start()
        {
            var editor = new TaskExecutor(IO, Title, Data.Tasks, true);
            IO.Run(editor.Edit);
        }
    }
}