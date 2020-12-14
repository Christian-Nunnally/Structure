using System;
using System.Collections.Generic;
using System.Linq;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class PrestiegeModule : Module
    {
        public override int RequiredLevel => 2;

        public override IEnumerable<(char, string, Action)> GetOptions()
        {
            var results = new List<(char, string, Action)>();
            if (Level > 0) results.Add(('p', "prestiege", () => PromptYesNo($"Restart and gain {Level} prestiege points?", () => Run(IncrementPrestiege, 3))));
            if (Prestiege > 0) results.Add(('s', "spend prestiege", () => PromptYesNo($"Spend 1/{Prestiege} prestiege points on 10 blades of grass?", () => Run(Buy10Grass, 3))));
            return results;
        }

        private void Buy10Grass()
        {
            Grass += 10;
            Prestiege -= 1;
            Write($"You now have {Grass} blades of grass.");
            Write($"Each blade of grass reduces added toxins by 1 per day.");
        }

        private void IncrementPrestiege()
        {
            Prestiege += Level;
            LifetimePrestiege += Level;
            Level = 0;
            XP = 0;
            Points = 0;
            EnabledModules.All(x => Program.Modules.Where(y => y.GetType().Name == x).All(z => z.Disable()));
            EnabledModules.Clear();
        }
    }
}