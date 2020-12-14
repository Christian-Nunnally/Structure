using System;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class CharacterBonusModule : Module
    {
        private DateTime _lastCharacterBonusTime = DateTime.Now;

        public override int RequiredLevel => 0;

        public override void Enable()
        {
            Program.RegularActions.Add(UpdateCharacterBonus);
            Program.RegularActions.Add(GrantCharacterBonus);
        }

        private void UpdateCharacterBonus()
        {
            var adjustedSum = 0;
            foreach (var codeLength in Utility.CodeLengths)
            {
                adjustedSum += Math.Max(0, codeLength - CharacterBonusPerFile);
            }
            var lengthChange = adjustedSum - LastCodeLength;
            if (lengthChange != 0)
            {
                Run(() =>
                {
                    LastCodeLength = adjustedSum;
                    CharacterBonus -= lengthChange;
                }, 3);
            }
        }

        private void GrantCharacterBonus()
        {
            if ((DateTime.Now - _lastCharacterBonusTime).TotalMinutes > 5)
            {
                Run(() =>
                {
                    var bonus = (int)(5f * ((LifetimePrestiege / 100f) + 1));
                    CharacterBonus += bonus;
                    _lastCharacterBonusTime = DateTime.Now;
                }, 1);
            }
        }
    }
}