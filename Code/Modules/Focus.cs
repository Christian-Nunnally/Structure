using System;
using System.Linq;

namespace Structure
{
    internal class Focus : Module
    {
        private UserAction _userAction;

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.T, _userAction);
        }

        public override void Enable()
        {
            _userAction = Hotkey.Add(ConsoleKey.T, new UserAction("StartFocus", Start));
        }

        private void Start()
        {
            var allTaskIds = Data.ActiveTaskTree.Select(x => x.Value.ID).ToList();
            var leafTaskIds = allTaskIds.Where(x => !Data.ActiveTaskTree.Any(y => x == y.Value.ParentID));
        }
    }
}