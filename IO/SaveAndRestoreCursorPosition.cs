using Structure.IO.Output;
using Structure.Structure.Utility;

namespace Structure.IO
{
    public class SaveAndRestoreCursorPosition : DisposableAction
    {
        private (int CursorLeft, int CursorTop) _position;
        private readonly IProgramOutput _programOutput;

        public SaveAndRestoreCursorPosition(IProgramOutput programOutput)
        {
            _programOutput = programOutput;
            SaveCursorPosition();
            DisposeAction = RestoreCursorPosition;
        }

        private void SaveCursorPosition()
        {
            _position = (_programOutput.CursorLeft, _programOutput.CursorTop);
            _programOutput.CursorVisible = false;
        }

        private void RestoreCursorPosition()
        {
            _programOutput.CursorLeft = _position.CursorLeft;
            _programOutput.CursorTop = _position.CursorTop;
            _programOutput.CursorVisible = true;
        }
    }
}
