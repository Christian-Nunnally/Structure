using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class LevelUpModule : Module
    {
        public override int RequiredLevel => 0;

        public override void Enable()
        {
            Program.RegularActions.Add(TryToLevelUp);
        }

        private void TryToLevelUp()
        {
            if (XP >= Utility.XPForNextLevel)
            {
                Run(() => PromptYesNo("You have enough XP to level up, want to?", LevelUp), 5);
            }
        }

        private void LevelUp()
        {
            XP -= Utility.XPForNextLevel;
            Level++;
            CharacterBonus += Level * Level;
        }
    }
}