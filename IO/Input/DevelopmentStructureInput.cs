using Structur.IO;
using Structur.IO.Output;

namespace Structur.IO.Input
{
    public class DevelopmentStructureInput : StructureInput
    {
        public DevelopmentStructureInput(StructureIO io, IProgramInput sourceInput, IProgramOutput sourceOutput, INewsPrinter newsPrinter, bool loadSave) : base(io, sourceInput, sourceOutput, newsPrinter)
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