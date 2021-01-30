using System;
using System.Collections.Generic;
using System.Linq;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class TreeEditor
    {
        private int _cursor = 0;
        private string _parent;
        private bool _refreshDisplay;
        private bool _goBackIfNoChild;
        private PersistedTree<TaskItem> _tree;

        public TreeEditor(PersistedTree<TaskItem> tree)
        {
            _tree = tree;
        }

        public void DoTasks()
        {
            var tasks = GetChildren(_parent);
            ConsolidateRank(tasks);
            WriteTasks(tasks);
            if (tasks.Count == 0) { PromptToInsertTask(); Clear(); DoTasks(); }
            else ReadKey(x => DoTasksInteraction(x));
        }

        public void FocusTask(string taskId)
        {
            _parent = taskId;
            _goBackIfNoChild = false;
        }

        private static void ConsolidateRank(List<TaskItem> tasks) => tasks.All(t => t.Rank = tasks.IndexOf(t));

        private List<TaskItem> GetChildren(string parent) =>
            _tree.Where(x => x.Value.ParentID == parent)
                .Select(x => x.Value)
                .OrderBy(x => x.Rank)
                .ToList();

        private void IncrementTasksRanks(string parentID, int startingRank) =>
            GetChildren(parentID)
                .Where(x => x.Rank >= startingRank)
                .All(x => x.Rank++);

        private void WriteTasks(List<TaskItem> tasks) => tasks.All(t => Write($"{tasks.IndexOf(t)} {(tasks.IndexOf(t) == _cursor ? ">" : " ")} {t.Task}"));

        private void DoTasksInteraction(string key)
        {
            var tasks = GetChildren(_parent);
            if (_cursor < 0 || _cursor >= tasks.Count) _cursor = 0;
            var task = tasks[_cursor];

            var actions = new Dictionary<string, Action>
            {
                { "i", PromptToInsertTask },
                { "a", () => ReparentToGrandparent(task) },
                { "d", () => ParentUnderSibling(task, tasks) },
                { "{UpArrow}", CursorUp },
                { "{DownArrow}", CursorDown },
                { "{LeftArrow}", ViewParent },
                { "{RightArrow}", () => FocusTask(task.ID) },
                { "{Delete}", () => DeleteTask(task) },
                { "{Enter}", () => EnterPressed(task) },
                { "w", () => LowerTaskRank(task) },
                { "s", () => RaiseTaskRank(task) },
            };
            if (key == "{Escape}") return;
            if (actions.ContainsKey(key)) actions[key]();

            if (GetChildren(_parent).Count == 0 && _goBackIfNoChild)
            {
                ViewParent();
            }
            _goBackIfNoChild = true;
            Clear(_refreshDisplay);
            _refreshDisplay = true;
            DoTasks();
        }

        private void ReparentToGrandparent(TaskItem task)
        {
            task.ParentID = _tree[task.ParentID]?.ParentID;
        }

        private void ParentUnderSibling(TaskItem task, List<TaskItem> siblings)
        {
            if (siblings.Contains(task)) siblings.Remove(task);
            if (!siblings.Any()) return;
            int i = 0;
            PromptOptions($"Select the new parent for {task.Task}", false, siblings.Select(s => new UserAction($"{i++} {s.Task}", () => task.ParentID = s.ID)).ToArray());
        }

        private void EnterPressed(TaskItem task)
        {
            if (GetChildren(task.ID).Count > 0) FocusTask(task.ID);
            else ConfirmTaskCompletion(task);
        }

        private void LowerTaskRank(TaskItem task)
        {
            var tasks = GetChildren(task.ParentID);
            var thisTaskIndex = tasks.IndexOf(task);
            if (thisTaskIndex > 0 && tasks.Count > 1)
            {
                var otherTask = tasks[thisTaskIndex - 1];
                otherTask.Rank++;
                task.Rank--;
                _cursor--;
            }
        }

        private void RaiseTaskRank(TaskItem task)
        {
            var tasks = GetChildren(task.ParentID);
            var thisTaskIndex = tasks.IndexOf(task);
            if (thisTaskIndex < tasks.Count - 1)
            {
                var otherTask = tasks[thisTaskIndex + 1];
                otherTask.Rank--;
                task.Rank++;
                _cursor++;
            }
        }

        private void ConfirmTaskCompletion(TaskItem task) => PromptOptions($"\n{task.Task} completed?", true,
                new UserAction("no", () => { }),
                new UserAction("yes", () => CompleteTask(task)));

        private void DeleteTask(TaskItem task)
        {
            _tree.Remove(task.ID);
        }

        private void PromptToInsertTask()
        {
            var index = GetChildren(_parent).Count;
            WriteNoLine($"\nInsert task #{index}: ");
            Read(s => AddTask(s, _parent, index), ConsoleKey.Enter, ConsoleKey.LeftArrow);
            if (GetChildren(_parent).Count == 0) ViewParent();
        }

        private void CursorDown()
        {
            _cursor = Math.Min(_cursor + 1, GetChildren(_parent).Count - 1);
            _refreshDisplay = false;
        }

        private void CursorUp()
        {
            _cursor = Math.Max(0, _cursor - 1);
            _refreshDisplay = false;
        }

        private void ViewParent()
        {
            var currentParent = _tree.Get(_parent);
            _parent = currentParent?.ParentID;
            _cursor = GetChildren(_parent).IndexOf(currentParent);
        }

        private void CompleteTask(TaskItem task)
        {
            task.CompletedDate = DateTime.Now;
            _tree.Remove(task.ID);
            CompletedTasks.Add(task.Task);
            CompletedTaskTree.Set(task);
            Points++;
            CharacterBonus++;
            XP++;
        }

        private void AddTask(string description, string parentID, int rank)
        {
            if (string.IsNullOrEmpty(description))
            {
                return;
            }
            var task = new TaskItem
            {
                ParentID = parentID,
                Task = description,
                Rank = rank
            };
            IncrementTasksRanks(parentID, rank);
            _tree.Set(task.ID, task);
        }
    }
}