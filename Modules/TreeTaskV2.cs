using System;

namespace Structure
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
            _doTasks = Hotkey.Add(ConsoleKey.T, new UserAction("Do tasks", Start));
        }

        private void Start()
        {
            var editor = new TaskEditor(IO, Data);
            IO.Run(editor.Edit);
        }
    }
}