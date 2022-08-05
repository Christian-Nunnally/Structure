namespace Structur.IO.Output
{
    public interface IProgramOutput
    {
        public int CursorLeft { get; set; }

        public int CursorTop { get; set; }

        public bool CursorVisible { get; set; }

        int Width { get; }

        int Height { get; }

        void Write(string text);

        void WriteLine(string text);

        void Clear();
    }
}