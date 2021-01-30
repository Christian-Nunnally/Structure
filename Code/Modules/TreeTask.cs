using System;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class TreeTask : Module
    {
        private UserAction _doTasks;

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.T, _doTasks);
        }

        public override void Enable()
        {
            _doTasks = Hotkey.Add(ConsoleKey.T, new UserAction("Do tasks", Start));
        }

        private void Start() => Run(() => new TreeEditor(ActiveTaskTree).DoTasks());
    }
}