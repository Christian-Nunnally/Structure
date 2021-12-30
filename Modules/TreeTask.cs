using System;
using System.Collections.Generic;

namespace Structure
{
    internal class TreeTask : Module
    {
        public static List<TaskEditor> OpenEditors = new List<TaskEditor>();
        private UserAction _doTasks;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.T, _doTasks);
        }

        protected override void OnEnable()
        {
            _doTasks = Hotkey.Add(ConsoleKey.T, new UserAction("Do tasks", Start));
        }

        private void Start()
        {
            var editor = new TaskEditor(IO, CurrentData);
            OpenEditors.Add(editor);
            IO.Run(() => editor.Edit());
            if (OpenEditors.Count > 0) OpenEditors.RemoveAt(OpenEditors.Count - 1);
        }
    }
}