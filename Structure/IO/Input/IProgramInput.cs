namespace Structur.IO.Input
{
    public interface IProgramInput
    {
        public ProgramInputData ReadInput();

        public void RemoveLastInput();

        public bool IsInputAvailable();
    }
}