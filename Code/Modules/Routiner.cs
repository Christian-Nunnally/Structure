using System;
using System.Collections.Generic;
using System.Linq;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class Routiner : Module
    {
        private List<string> _activeRoutines = new List<string>();

        private Dictionary<string, string[]> _routines = new Dictionary<string, string[]>
        {
            { "Morning", new string[]
                { "Drink glass of water",
                  "Shower",
                  "Brush teeth", } },
            { "Night", new string[]
                { "Brush teeth",
                  "Wash face",
                  "Drink a glass of water",
                  "Prepare a glass of water for the morning" } },
            { "Start work", new string[]
                { "Drink a glass of water",
                  "Check email",
                  "Check teams",
                  "Add calander items to todo list" } },
            { "End work", new string[]
                { "Drink a glass of water",
                  "Check email",
                  "Check teams",
                  "Close unneeded windows"} },
        };

        private UserAction _action;

        public Routiner()
        {
            ActiveTaskTree.ItemRemoved += TaskCompleted;
        }

        private UserAction[] RoutineOptions => _routines.Select(r => new UserAction(r.Key, () => StartRoutine(r.Key, r.Value))).ToArray();

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.R, _action);
        }

        public override void Enable()
        {
            _action = Hotkey.Add(ConsoleKey.R, new UserAction("Routines", DoRoutine));
        }

        private void DoRoutine()
        {
            if (_activeRoutines.Any())
            {
                Write("Active routines:");
                _activeRoutines.All(r => Write($"{r} in progress"));
                Write();
            }
            PromptOptions("Pick a routine", false, RoutineOptions);
        }

        private void StartRoutine(string name, string[] tasks)
        {
            name = $"{name} routine";
            _activeRoutines.Add(name);
            Write($"Starting {name}");

            ActiveTaskTree.Set(name, new TaskItem { ID = name, Task = name, Rank = 300 });
            for (int i = 0; i < tasks.Count(); i++)
            {
                var guid = Guid.NewGuid().ToString();
                ActiveTaskTree.Set(guid, new TaskItem { ID = guid, ParentID = name, Rank = i, Task = tasks[i] });
            }
            Run(() => StartTreeTaskForRoutine(name));
        }

        private void StartTreeTaskForRoutine(string name)
        {
            var tree = new TreeEditor(ActiveTaskTree);
            tree.FocusTask(name);
            tree.DoTasks();
        }

        private void TaskCompleted(string taskID, TaskItem removedTask)
        {
            if (removedTask.CompletedDate == new DateTime())
            {
                return;
            }
            if (_activeRoutines.Contains(removedTask.ID))
            {
                FinishRoutine(removedTask.ID);
            }
        }

        private void FinishRoutine(string routine)
        {
            _activeRoutines.Remove(routine);
            News($"Finished {routine}.");
            Points += 2;
            XP += 5;
            CharacterBonus += 5;
            FinishedRoutines.Add($"{DateTime.Now} {routine}");
        }
    }
}