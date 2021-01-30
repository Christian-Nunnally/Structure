using System;
using System.Linq;
using static Structure.Data;

namespace Structure
{
    internal class Prestieger : Module
    {
        private UserAction _userAction;

        public override void Enable()
        {
            _userAction = Hotkey.Add(ConsoleKey.Q, new UserAction("Prestiege", () => IO.PromptYesNo($"Restart and gain {Level} prestiege points?", IncrementPrestiege)));
        }

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.Q, _userAction);
        }

        private void IncrementPrestiege()
        {
            Prestiege++;
            Level = 0;
            XP = 0;
            Points = 0;
            CharacterBonus = -LastCodeLength;
            CharacterBonusPerFile = 0;
            EnabledModules.All(x => Modules.UserModules.Where(y => y.GetType().Name == x).All(z => z.Disable()));
            EnabledModules.Clear();
        }
    }
}