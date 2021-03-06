using System;

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

        private void Start() => IO.Run(() => new TaskEditor().Edit());
    }
}