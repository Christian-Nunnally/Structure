﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Structur.Program;
using Structur.Program.Utilities;

namespace Structur.IO
{
    public class Hotkey
    {
        private readonly Dictionary<ConsoleKey, List<UserAction>> _hotkeys = new();

        public void Print(StructureIO io) => _hotkeys.All(x => x.Value.All(y => io.Write($"ctrl + {$"{x.Key}".ToLower(CultureInfo.CurrentCulture)}: {y.Description}")));

        public void Execute(ConsoleKeyInfo key, StructureIO io)
        {
            Contract.Requires(io != null);
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control)
                && _hotkeys.TryGetValue(key.Key, out var actions))
            {
                if (actions.Count == 1)
                {
                    io.Run(actions.First().Action);
                }
                else
                {
                    io.Run(() => io.ReadOptions($"ctrl + {$"{key.Key}".ToLower(CultureInfo.CurrentCulture)} pressed, select option:", "", actions.ToArray()));
                }
            }
        }

        public void Add(UserAction action) => Add(action.Hotkey.Key, action);

        public void Add(ConsoleKey key, UserAction action)
        {
            if (_hotkeys.ContainsKey(key)) _hotkeys[key].Add(action);
            else _hotkeys.Add(key, new List<UserAction> { action });
        }

        public void Remove(UserAction action) => Remove(action.Hotkey.Key, action);

        public void Remove(ConsoleKey key, UserAction action)
        {
            if (!_hotkeys.TryGetValue(key, out var list)) return;
            if (list.Contains(action)) list.Remove(action);
            if (!list.Any()) _hotkeys.Remove(key);
        }
    }
}