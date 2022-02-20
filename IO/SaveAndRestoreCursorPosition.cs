using Structure.IO.Output;
using Structure.Structure;

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

        private void SaveCursorPosition() => _position = (_programOutput.CursorLeft, _programOutput.CursorTop);

        private void RestoreCursorPosition()
        {
            _programOutput.CursorLeft = _position.CursorLeft;
            _programOutput.CursorTop = _position.CursorTop;
        }
    }
}
