namespace Structure
{
    public class NoOpOutput : IProgramOutput
    {
        public int CursorLeft { get; set; }

        public int CursorTop { get; set; }

        public void Clear() { }

        public void SetCursorPosition(int left, int top) { }

        public void Write(string text) { }

        public void WriteLine(string text) { }
    }
}