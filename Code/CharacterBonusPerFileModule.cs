using System;
using System.Collections.Generic;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class CharacterBonusPerFileModule : Module
    {
        public override int RequiredLevel => 1;

        public override IEnumerable<(char, string, Action)> GetOptions()
        {
            return new List<(char, string, Action)> { ('c', "character bonus", IncreaseCharacterBonusPerFile) };
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
                PromptYesNo($"Spend {costOfNextLevel}/{Points} points on +{increaseAmount} character bonus per file?", () => Run(() => IncreaseCharacterBonusPerFile(increaseAmount, costOfNextLevel), 3));
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