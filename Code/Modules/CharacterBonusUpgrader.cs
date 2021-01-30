using System;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class CharacterBonusUpgrader : Module
    {
        private UserAction _action;

        public override void Enable()
        {
            _action = Hotkey.Add(ConsoleKey.Q, new UserAction("Increase character bonus", IncreaseCharacterBonusPerFile));
        }

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.Q, _action);
        }

        private void IncreaseCharacterBonusPerFile()
        {
            var increaseAmount = 50;
            var currentBonus = CharacterBonusPerFile;
            var bonusLevel = 0;
            while (currentBonus > 0)
            {
                currentBonus -= increaseAmount;
                if (increaseAmount > 1) increaseAmount--;
                bonusLevel++;
            }
            var costOfNextLevel = Utility.ExperienceForLevel(bonusLevel, 10, 65, 50);
            if (costOfNextLevel <= Points)
            {
                PromptYesNo($"Spend {costOfNextLevel}/{Points} points on +{increaseAmount} character bonus per file?", () => IncreaseCharacterBonusPerFile(increaseAmount, costOfNextLevel));
            }
            else
            {
                Write($"You require {Points}+({costOfNextLevel - Points}) points to upgrade the character bonus per file to {CharacterBonusPerFile}+({increaseAmount}).");
                ReadAny();
            }
        }

        private void IncreaseCharacterBonusPerFile(int amount, int cost)
        {
            Points -= cost;
            CharacterBonusPerFile += amount;
        }
    }
}