using Structure.IO;

namespace Structure.IO.Input
{
    public class DevelopmentStructureInput : StructureInput
    {
        public DevelopmentStructureInput(StructureIO io, IProgramInput sourceInput, INewsPrinter newsPrinter, bool loadSave) : base(io, sourceInput, newsPrinter)
        {
            if (loadSave)
            {
                InitializeNewInputFromSavedSessions(io, newsPrinter);
            }
            else
            {
                InputSource = new ChainedInput();
            }
            InputSource.AddInput(sourceInput);
        }
    }
}