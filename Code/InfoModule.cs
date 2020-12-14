using System;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class InfoModule : Module
    {
        public override int RequiredLevel => 1;

        public override void Disable()
        {
            Hotkeys.Remove(ConsoleKey.I);
        }

        public override void Enable()
        {
            Hotkeys.Add(ConsoleKey.I, (PrintInfo, "Info"));
        }

        private void PrintInfo()
        {
            Write($"Lifetime Prestiege = {LifetimePrestiege}");
            Write($"Prestiege = {Prestiege}");
            Write($"Level = {Level}");
            Write($"XP = {XP}/{Utility.XPForNextLevel}");
            Write($"Points = {Points}");
            Write($"Character Bonus = {CharacterBonus}");
            Write($"Character Bonus/File = {CharacterBonusPerFile}");
            Write($"You have {Grass} blades of grass.");
            Write($"Toxins = {Toxins}");
            Write($"Length of the code = {LastCodeLength}");
            ReadAny();
        }
    }
}