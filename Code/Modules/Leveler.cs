using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class Leveler : Module
    {
        public override void Enable()
        {
            Program.RegularActions.Add(TryToLevelUp);
        }

        private void TryToLevelUp()
        {
            if (XP >= Utility.XPForNextLevel)
            {
                Run(() => PromptYesNo("You have enough XP to level up, want to?", LevelUp));
            }
        }

        private void LevelUp()
        {
            XP -= Utility.XPForNextLevel;
            Data.Level++;
            CharacterBonus += Data.Level * Data.Level;
        }
    }
}