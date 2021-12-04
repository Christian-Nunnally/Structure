using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure
{
    public static class Hotkey
    {
        public static Dictionary<ConsoleKey, List<UserAction>> Hotkeys = new Dictionary<ConsoleKey, List<UserAction>>();

        public static void Print() => Hotkeys.All(x => x.Value.All(y => IO.Write($"ctrl + {$"{x.Key}".ToLower()}: {y.Description}")));

        public static void Execute(ConsoleKeyInfo key)
        {
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control)
                && Hotkeys.TryGetValue(key.Key, out var actions))
            {
                if (actions.Count == 1)
                {
                    IO.Run(actions.First().Action);
                }
                else
                {
                    IO.Run(() => IO.PromptOptions($"ctrl + {$"{key.Key}".ToLower()} pressed, select option:", false, actions.ToArray()));
                }
            }
        }

        public static UserAction Add(ConsoleKey key, UserAction action)
        {
            if (Hotkeys.ContainsKey(key)) Hotkeys[key].Add(action);
            else Hotkeys.Add(key, new List<UserAction> { action });
            return action;
        }

        public static void Remove(ConsoleKey key, UserAction action)
        {
            if (!Hotkeys.TryGetValue(key, out var list)) return;
            if (list.Contains(action)) list.Remove(action);
            if (!list.Any()) Hotkeys.Remove(key);
        }
    }
}