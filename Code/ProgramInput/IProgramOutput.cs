namespace Structure
{
    public interface IProgramOutput
    {
        public int CursorLeft { get; set; }

        public int CursorTop { get; set; }

        void Write(string text);

        void WriteLine(string text);

        void SetCursorPosition(int left, int top);

        void Clear();
    }
}