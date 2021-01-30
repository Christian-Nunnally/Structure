namespace Structure
{
    internal class Toxin : Module
    {
        public override void Enable()
        {
            Program.RegularActions.Add(UpdateToxins);
        }

        private void UpdateToxins()
        {
            if (FileIO.GetLastWriteTime("Toxins").Date != System.DateTime.Today.Date)
            {
                Data.Toxins += Utility.CodeLength;
            }
        }
    }
}