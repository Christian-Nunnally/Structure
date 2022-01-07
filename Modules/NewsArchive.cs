using System;
using System.Collections.Generic;

namespace Structure
{
    internal class NewsArchive : StructureModule
    {
        private UserAction _action;

        protected override void OnEnable()
        {
            _action = Hotkey.Add(ConsoleKey.N, new UserAction("News", () => { }));
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.N, _action);
        }
    }
}