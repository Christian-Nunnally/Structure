using System;

namespace Structure.Modules
{
    public class TaskHistoryInformation : Module
    {
        private UserAction _startAction;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.H, _startAction);
        }

        protected override void OnEnable()
        {
            _startAction = Hotkey.Add(ConsoleKey.H, new UserAction("Task history", Start));
        }

        private void Start()
        {
            throw new NotImplementedException();
        }
    }
}
