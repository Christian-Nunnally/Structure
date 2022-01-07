using System;

namespace Structure
{
    internal class Information : StructureModule
    {
        private UserAction _userAction;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.I, _userAction);
        }

        protected override void OnEnable()
        {
            _userAction = Hotkey.Add(ConsoleKey.I, new UserAction("Info", PrintInfo));
        }

        private void PrintInfo()
        {
            IO.Write($"Level = {Data.Level}");
            IO.Write($"XP = {Data.XP}/{Utility.XPForNextLevel(Data)}");
            IO.Write($"Points = {Data.Points}");
            // IO.ReadAny();
        }
    }
}