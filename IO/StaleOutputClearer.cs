using Structure.IO.Output;
using System;

namespace Structure.IO
{
    class StaleOutputClearer : IBackgroundProcess
    {
        private int _currentIndex = 0;
        private bool _needsClearing = false;
        private bool _isCurrentClearValid = false;
        private int _x = 0;
        private int _y = 1;

        public bool DoProcess(StructureIO io)
        {
            if (_needsClearing) ClearStaleOutput(io.CurrentBuffer.ToString(), io.ProgramOutput);
            return _needsClearing;
        }

        private void ClearStaleOutput(string buffer, IProgramOutput output)
        {
            using var savePosition = new SaveAndRestoreCursorPosition(output);


            if (!_isCurrentClearValid)
            {
                _currentIndex = 0;
                _isCurrentClearValid = true;
                _x = 0;
                _y = 1;
            }

            for (int i = _currentIndex; i < buffer.Length; i++)
            {
                var character = buffer[i];
                output.SetCursorPosition(Math.Min(_x++, output.Width - 1), _y);
                if (!char.IsWhiteSpace(character)) continue;

                if (character == '\n' || _x >= output.Width)
                {
                    while (_x++ < output.Width) output.Write(" ");
                    _y++;
                    _x = 0;
                    _currentIndex = i + 1;
                    return;
                }
                else output.Write(" ");
            }

            var spaces = string.Empty;
            for (int i = 0; i < output.Width; i++) spaces += " ";
            var ending = string.Empty;
            for (; _y < output.Height - 1; _y++) ending += spaces;
            output.Write(ending);
            _needsClearing = false;
        }

        public void ClearStaleOutput(StructureIO io)
        {
            _currentIndex = 0;
            _needsClearing = true;
            _isCurrentClearValid = false;
        }
    }
}
