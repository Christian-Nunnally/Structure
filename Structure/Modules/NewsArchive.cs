﻿using System;
using System.Collections.Generic;

namespace Structure
{
    internal class NewsArchive : Module
    {
        private UserAction _action;

        protected override void OnEnable()
        {
            _action = Hotkey.Add(ConsoleKey.N, new UserAction("News", BrowseNews));
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.N, _action);
        }

        private void BrowseNews()
        {
            var news = new List<object>();
            IO.NewsArchive.All(news.Add);
            news.Reverse();
            new ListViewer("News archive:", news, 10).ViewList();
        }
    }
}