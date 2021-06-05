using System;
using System.Collections.Generic;

namespace Structure
{
    internal class TreeTask : Module
    {
        public static List<TaskEditor> OpenEditors = new List<TaskEditor>();
        private UserAction _doTasks;

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.T, _doTasks);
        }

        public override void Enable()
        {
            _doTasks = Hotkey.Add(ConsoleKey.T, new UserAction("Do tasks", Start));
        }

        private void Start()
        {
            var editor = new TaskEditor();
            OpenEditors.Add(editor);
            IO.Run(() => editor.Edit());
            if (OpenEditors.Count > 0) OpenEditors.RemoveAt(OpenEditors.Count - 1);
        }
    }
}