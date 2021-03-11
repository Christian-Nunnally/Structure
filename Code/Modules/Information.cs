﻿using System;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class Information : Module
    {
        private UserAction _userAction;

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.I, _userAction);
        }

        public override void Enable()
        {
            _userAction = Hotkey.Add(ConsoleKey.I, new UserAction("Info", PrintInfo));
        }

        private void PrintInfo()
        {
            Write($"Level = {Data.Level}");
            Write($"XP = {XP}/{Utility.XPForNextLevel}");
            Write($"Points = {Points}");
            ReadAny();
        }
    }
}