using System;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class ToxinModule : Module
    {
        public override int RequiredLevel => 0;

        public override void Enable()
        {
            Program.RegularActions.Add(UpdateToxins);
        }

        private void UpdateToxins()
        {
            if (GetLastWriteTime("Toxins").Date != DateTime.Today.Date)
            {
                Run(() =>
                {
                    var prestiegeBonus = (LifetimePrestiege / 1000f) + 1;
                    var grassAbsorption = (int)(Grass * prestiegeBonus);
                    var addedToxins = Math.Max(0, Utility.CodeLength - grassAbsorption);
                    Write($"Added toxins per day = {addedToxins} ({grassAbsorption} toxins being absorbed by grass blades)");
                    Toxins += addedToxins;
                }, 2);
            }
        }
    }
}