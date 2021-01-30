using System;
using System.Linq;
using static Structure.Data;

namespace Structure
{
    internal class CharacterBonusModule : Module
    {
        private DateTime _lastBonusTime = DateTime.Now;

        public override void Enable()
        {
            Program.RegularActions.Add(Update);
        }

        private void Update()
        {
            var disabledModules = Modules.UserModules.Where(m => !EnabledModules.Contains(m.Name)).Select(m => m.Name);
            var adjustedSum = Utility.CodeLengths
                .Where(f => !disabledModules.Any(x => x == f.FileName))
                .Select(x => Math.Max(0, x.Length - CharacterBonusPerFile))
                .Sum();
            var change = adjustedSum - LastCodeLength;
            if (change != 0)
            {
                LastCodeLength = adjustedSum;
                CharacterBonus -= change;
            }

            if ((DateTime.Now - _lastBonusTime).TotalMinutes > 5)
            {
                CharacterBonus += (int)(5 + 5f * Data.Prestiege / 100f);
                _lastBonusTime = DateTime.Now;
            }
        }
    }
}