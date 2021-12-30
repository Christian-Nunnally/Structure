﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure
{
    public class Hotkey
    {
        private readonly Dictionary<ConsoleKey, List<UserAction>> _hotkeys = new Dictionary<ConsoleKey, List<UserAction>>();

        public void Print(StructureIO io) => _hotkeys.All(x => x.Value.All(y => io.Write($"ctrl + {$"{x.Key}".ToLower()}: {y.Description}")));

        public void Execute(ConsoleKeyInfo key, StructureIO io)
        {
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control)
                && _hotkeys.TryGetValue(key.Key, out var actions))
            {
                if (actions.Count == 1)
                {
                    io.Run(actions.First().Action);
                }
                else
                {
                    io.Run(() => io.PromptOptions($"ctrl + {$"{key.Key}".ToLower()} pressed, select option:", false, actions.ToArray()));
                }
            }
        }

        public UserAction Add(ConsoleKey key, UserAction action)
        {
            if (_hotkeys.ContainsKey(key)) _hotkeys[key].Add(action);
            else _hotkeys.Add(key, new List<UserAction> { action });
            return action;
        }

        public void Remove(ConsoleKey key, UserAction action)
        {
            if (!_hotkeys.TryGetValue(key, out var list)) return;
            if (list.Contains(action)) list.Remove(action);
            if (!list.Any()) _hotkeys.Remove(key);
        }
    }
}