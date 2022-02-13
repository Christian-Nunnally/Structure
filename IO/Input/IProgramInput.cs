namespace Structure.IO.Input
{
    public interface IProgramInput
    {
        public ProgramInputData ReadKey();

        public void RemoveLastReadKey();

        public bool IsKeyAvailable();
    }
}