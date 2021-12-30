using System;

namespace Structure
{
    internal class Information : Module
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
            IO.Write($"Level = {CurrentData.Level}");
            IO.Write($"XP = {CurrentData.XP}/{Utility.XPForNextLevel(CurrentData)}");
            IO.Write($"Points = {CurrentData.Points}");
            IO.ReadAny();
        }
    }
}