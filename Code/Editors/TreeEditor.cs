using System;
using System.Collections.Generic;
using System.Linq;
using static Structure.IO;

namespace Structure
{
    public class TreeEditor<ItemType> where ItemType : Node
    {
        public readonly List<(string, Action)> CustomActions = new List<(string, Action)>();
        public Action<ItemType> EnterPressedOnParentAction;
        public Action<ItemType> EnterPressedOnLeafAction;
        public Action NoChildrenAction;

        protected string _currentParent;
        protected bool EnableReparenting = true;
        protected PersistedTree<ItemType> Tree;
        protected bool ShowChildren;
        protected bool ShouldExit;
        protected int _cursor = 0;
        private readonly string _prompt;
        private bool _refreshDisplay;
        private bool _goBackIfNoChild;

        public TreeEditor(string prompt, PersistedTree<ItemType> tree)
        {
            EnterPressedOnParentAction = SetParent;
            EnterPressedOnLeafAction = SetParent;
            NoChildrenAction = () => { News("No children"); ViewParent(); };
            _prompt = prompt;
            Tree = tree;
        }

        public ItemType CurrentParent => _currentParent == null ? null : Tree.Get(_currentParent);

        public int NumberOfVisibleTasks => GetChildren(_currentParent).Count;

        public void Edit()
        {
            var children = GetChildren(_currentParent);
            ConsolidateRank(children);
            WriteHeader();
            WriteTasks(_cursor, children, "");
            if (ShouldExit) return;
            if (children.Count == 0) { NoChildrenAction(); Clear(); Edit(); }
            else ReadKey(x => DoTasksInteraction(x));
        }

        public void SetParent(ItemType item)
        {
            _currentParent = item.ID;
            _goBackIfNoChild = false;
        }

        public void ViewParent()
        {
            if (_currentParent == null)
            {
                ShouldExit = true;
            }
            var currentParent = Tree.Get(_currentParent);
            _currentParent = currentParent?.ParentID;
            SetCursor(GetChildren(_currentParent).IndexOf(currentParent));
        }

        public List<ItemType> GetChildren(string parent) =>
            Tree.Where(x => x.Value.ParentID == parent)
                .Select(x => x.Value)
                .OrderBy(x => x.Rank)
                .ToList();

        protected void EnableDefaultInsertFunctionality(string insertPrompt)
        {
            CustomActions.Add(("i", (Action)(() => IO.Run(PromptToInsertNode(insertPrompt, DefaultNodeFactory)))));
            NoChildrenAction = PromptToInsertNode(insertPrompt, DefaultNodeFactory);
        }

        private static void ConsolidateRank(List<ItemType> tasks) => tasks.All(t => t.Rank = tasks.IndexOf(t));

        private void WriteHeader()
        {
            if (!string.IsNullOrWhiteSpace(_prompt))
            {
                Write(_prompt);
                Write();
            }

            var atParentKey = _currentParent;
            var parents = new List<string>();
            while (!string.IsNullOrWhiteSpace(atParentKey))
            {
                var atParent = Tree.Get(atParentKey);
                parents.Add(atParent.ToString());
                atParentKey = atParent.ParentID;
            }
            parents.Reverse();
            foreach (var parent in parents)
            {
                WriteNoLine($"{parent} > ");
            }
            Write();
        }

        private Node DefaultNodeFactory(string task, string parentId, int rank) => new TaskItem
        {
            Task = task,
            ParentID = parentId,
            Rank = rank,
        };

        private void WriteTasks(int cursorIndex, List<ItemType> tasks, string spaces)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                var prefix = spaces.Length == 0 ? $"{i}{(i == cursorIndex ? " > " : "   ")}" : "    " + spaces;
                Write($"{prefix}{tasks[i]}");
                if (ShowChildren) WriteTasks(-1, GetChildren(tasks[i].ID), spaces + "    ");
            }
        }

        private void DoTasksInteraction(string key)
        {
            SetCursor(_cursor);
            var tasks = GetChildren(_currentParent);
            var task = tasks[_cursor];

            _refreshDisplay = true;

            var actions = new Dictionary<string, Action>
            {
                { "{UpArrow}", CursorUp },
                { "{DownArrow}", CursorDown },
                { "{LeftArrow}", ViewParent },
                { "{RightArrow}", () => SetParent(task) },
                { "{Delete}", () => DeleteTask(task) },
                { "{Enter}", () => EnterPressed(task) },
                { "w", () => LowerTaskRank(task) },
                { "s", () => RaiseItemRank(task) },
            };
            if (EnableReparenting)
            {
                actions.Add("a", () => ReparentToGrandparent(task));
                actions.Add("d", () => ParentUnderSibling(task, tasks));
            }
            for (var i = 0; i < 9; i++)
            {
                var b = i;
                actions.Add($"{i}", () => SetCursor(b));
            }
            CustomActions.All(x => actions.Add($"{x.Item1}", x.Item2));
            if (key == "{Escape}") return;
            if (actions.ContainsKey(key)) actions[key]();

            if (GetChildren(_currentParent).Count == 0 && _goBackIfNoChild)
            {
                ViewParent();
            }
            _goBackIfNoChild = true;
            Clear(_refreshDisplay);
            Edit();
        }

        private void SetCursor(int index) => _cursor = Math.Max(0, Math.Min(index, GetChildren(_currentParent).Count - 1));

        private void ReparentToGrandparent(ItemType task)
        {
            task.ParentID = Tree[task.ParentID]?.ParentID;
        }

        private void ParentUnderSibling(ItemType task, List<ItemType> siblings)
        {
            if (siblings.Contains(task)) siblings.Remove(task);
            if (!siblings.Any()) return;
            int i = 0;
            PromptOptions($"Select the new parent for {task}", false, siblings.Select(s => new UserAction($"{i++} {s}", () => task.ParentID = s.ID)).ToArray());
        }

        private void EnterPressed(ItemType item) => (IsParent(item) ? EnterPressedOnParentAction : EnterPressedOnLeafAction)(item);

        private bool IsParent(ItemType item) => GetChildren(item.ID).Any();

        private void LowerTaskRank(ItemType task)
        {
            var tasks = GetChildren(task.ParentID);
            var thisTaskIndex = tasks.IndexOf(task);
            if (thisTaskIndex > 0 && tasks.Count > 1)
            {
                var otherTask = tasks[thisTaskIndex - 1];
                otherTask.Rank++;
                task.Rank--;
                SetCursor(_cursor - 1);
            }
        }

        private void RaiseItemRank(ItemType item)
        {
            var tasks = GetChildren(item.ParentID);
            var thisTaskIndex = tasks.IndexOf(item);
            if (thisTaskIndex < tasks.Count - 1)
            {
                var otherTask = tasks[thisTaskIndex + 1];
                otherTask.Rank--;
                item.Rank++;
                SetCursor(_cursor + 1);
            }
        }

        private void DeleteTask(ItemType task)
        {
            Tree.Remove(task.ID);
        }

        private void CursorDown()
        {
            SetCursor(_cursor + 1);
            _refreshDisplay = false;
        }

        private void CursorUp()
        {
            SetCursor(_cursor - 1);
            _refreshDisplay = false;
        }

        private Action PromptToInsertNode(string insertPrompt, Func<string, string, int, Node> nodeFactory) => () =>
        {
            var index = NumberOfVisibleTasks;
            IO.WriteNoLine($"\n{insertPrompt}: ");
            IO.Read(s => AddNode(nodeFactory, s, _currentParent, index), ConsoleKey.Enter, ConsoleKey.LeftArrow);
            if (NumberOfVisibleTasks == 0) ViewParent();
        };

        private void AddNode(Func<string, string, int, Node> nodeFactory, string description, string parentID, int rank)
        {
            if (string.IsNullOrEmpty(description))
            {
                return;
            }
            var node = nodeFactory(description, parentID, rank);
            Tree.Set(node as ItemType);
        }
    }
}