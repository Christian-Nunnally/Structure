using Structure.IO.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.IO
{
    public partial class NewsPrinter : INewsPrinter
    {
        private const int PRINT_SPEED = 1;
        private const int SCROLLING_TEXT_X_START_POSITION = 40;
        private const int SCROLLING_TEXT_X_END_POSITION = -80;
        private const int SCROLLING_TEXT_X_STOP_POSITION = 0;
        private const int SCROLLING_TEXT_Y_POSITION = 0;

        private readonly Queue<string> _newsQueue = new Queue<string>();
        private int _newsCursorX = 40;
        private string _currentNews;
        private bool _enabled;

        public bool PrintNews(IProgramOutput programOutput)
        {
            var isNewsCurrentlyPrinting = !string.IsNullOrEmpty(_currentNews);
            var isNewsWaitingToBePrinted = _newsQueue.Any();
            var isNewsFinishedPrinting = !isNewsWaitingToBePrinted && !isNewsCurrentlyPrinting;
            if (isNewsFinishedPrinting) return false;
            LoadOrPrintNews(programOutput, isNewsCurrentlyPrinting);
            return true;
        }

        private void LoadOrPrintNews(IProgramOutput programOutput, bool shouldLoadNextNews)
        {
            if (!shouldLoadNextNews) LoadNextPieceOfNews();
            if (IsTextScrollingOffSide()) ShortenCurrentNewsText();
            using (new SaveAndRestoreCursorPosition(programOutput))
            MoveCursorAndPrintCurrentNews(programOutput);
        }

        private void LoadNextPieceOfNews()
        {
            _currentNews = _newsQueue.Dequeue();
            _newsCursorX = SCROLLING_TEXT_X_START_POSITION;
        }

        private void ShortenCurrentNewsText()
        {
            _currentNews = _currentNews[Math.Min(PRINT_SPEED, _currentNews.Length)..];
        }

        private bool IsTextScrollingOffSide() => _newsCursorX < SCROLLING_TEXT_X_END_POSITION;

        private void MoveCursorAndPrintCurrentNews(IProgramOutput programOutput)
        {
            SetCursorToNextPosition(programOutput);
            var outputString = AddSpacesToCurrentNews(PRINT_SPEED);
            programOutput.Write(outputString);
        }

        private void SetCursorToNextPosition(IProgramOutput programOutput)
        {
            programOutput.CursorLeft = Math.Max(SCROLLING_TEXT_X_STOP_POSITION, _newsCursorX);
            programOutput.CursorTop = SCROLLING_TEXT_Y_POSITION;
            _newsCursorX -= PRINT_SPEED;
        }

        private string AddSpacesToCurrentNews(int PRINT_SPEED)
        {
            var outputString = _currentNews;
            for (int i = 0; i < PRINT_SPEED; i++) outputString += " ";
            return outputString;
        }

        public void EnqueueNews(string news)
        {
            if (_enabled) _newsQueue.Enqueue(news);
        }

        public bool DoProcess(StructureIO io)=> PrintNews(io?.ProgramOutput);

        public void Enable() => _enabled = true;

        public void Disable() => _enabled = false;
    }
}
