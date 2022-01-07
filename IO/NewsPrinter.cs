using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.IO
{
    public class NewsPrinter
    {
        private readonly Queue<string> _newsQueue = new Queue<string>();
        private string _currentNews;
        private int _newsCursorLeft = 40;

        internal bool PrintNews(IProgramOutput programOutput)
        {
            if (!_newsQueue.Any() && _currentNews == null) return false;
            if (_currentNews == null)
            {
                _currentNews = _newsQueue.Dequeue();
            }
            var cursorLeft = programOutput?.CursorLeft ?? 0;
            var cursorTop = programOutput?.CursorTop ?? 0;
            if (programOutput != null)
            {
                programOutput.CursorLeft = Math.Max(0, _newsCursorLeft);
                programOutput.CursorTop = 0;
            }
            _newsCursorLeft -= 2;
            if (programOutput != null)
            {
                programOutput.Write(_currentNews + "  ");
                programOutput.CursorLeft = cursorLeft;
                programOutput.CursorTop = cursorTop;
            }
            if (_newsCursorLeft < -80)
            {
                if (_currentNews.Length == 0)
                {
                    _currentNews = null;
                    _newsCursorLeft = 40;
                }
                else if (_currentNews.Length == 1)
                {
                    _currentNews = _currentNews[1..];
                }
                else
                {
                    _currentNews = _currentNews[2..];
                }
            }
            return true;
        }

        internal void ClearNews()
        {
            _newsQueue.Clear();
        }

        internal void EnqueueNews(string news)
        {
            _newsQueue.Enqueue(news);
        }
    }
}
