using System;
using System.IO;

namespace Structure
{
    public class FileOutput : IProgramOutput
    {
        readonly StreamWriter _streamWriter;

        public FileOutput(string filePath)
        {
            _streamWriter = new StreamWriter(filePath, false);
        }

        public int CursorLeft { get; set; }

        public int CursorTop { get; set; }

        public void Clear()
        {
        }

        public void SetCursorPosition(int left, int top)
        {
        }

        public void Write(string text)
        {
            _streamWriter.Write(text);
        }

        public void WriteLine(string text)
        {
            _streamWriter.WriteLine(text);
        }

        public void CloseFile()
        {
            _streamWriter.Close();
        }
    }
}