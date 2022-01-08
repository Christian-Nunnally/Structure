using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Structure
{
    public class TreeEditor<T> where T : Node
    {
        public List<(string, Action)> CustomActions { get; } = new List<(string, Action)>();
        public Action<T> EnterPressedOnParentAction { get; set; }
        public Action<T> EnterPressedOnLeafAction { get; set; }
        public Action NoChildrenAction { get; set; }
        protected Dictionary<Type, Dictionary<Type, Func<T, T>>> ItemConversionMap { get; } = new Dictionary<Type, Dictionary<Type, Func<T, T>>>();
        protected string CurrentParentCached { get; set; }
        protected bool EnableReparenting { get; set; } = true;
        protected NodeTreeCollection<T> Tree { get; set; }
        protected bool ShowChildren { get; set; }
        protected bool ShouldExit { get; set; }
        protected int Cursor { get; set; }

        private readonly string _prompt;
        private bool _refreshDisplay;
        private bool _goBackIfNoChild;
        private readonly StructureIO _io;

        public TreeEditor(StructureIO io, string prompt, NodeTreeCollection<T> tree)
        {
            EnterPressedOnParentAction = SetParent;
            EnterPressedOnLeafAction = SetParent;
            NoChildrenAction = () => { _io.News("No children"); ViewParent(); };
            CustomActions.Add(("t", ChangeItemType));
            _prompt = prompt;
            Tree = tree;
            _io = io;
        }

        public T CurrentParent => CurrentParentCached == null ? null : Tree.Get(CurrentParentCached);

        public int NumberOfVisibleTasks => GetChildren(CurrentParentCached).Count;

        public void Edit()
        {
            var children = GetChildren(CurrentParentCached);
            ConsolidateRank(children);
            WriteHeader();
            WriteTasks(Cursor, children, "");
            if (ShouldExit) return;
            if (children.Count == 0) { NoChildrenAction(); _io.Clear(clearConsole: true); Edit(); }
            // TODO: Use _io.PromptOptions instead
            else _io.Read(DoTasksInteraction, KeyGroups.MiscKeys, KeyGroups.NoKeys, echo: false);
        }

        public void SetParent(T item)
        {
            Contract.Requires(item != null);
            CurrentParentCached = item.ID;
            _goBackIfNoChild = false;
        }

        public void ViewParent()
        {
            if (CurrentParentCached == null)
            {
                ShouldExit = true;
            }
            var currentParent = Tree.Get(CurrentParentCached);
            CurrentParentCached = currentParent?.ParentID;
            SetCursor(GetChildren(CurrentParentCached).IndexOf(currentParent));
        }

        public List<T> GetChildren(string parent) =>
            Tree.Where(x => x.Value.ParentID == parent)
                .Select(x => x.Value)
                .OrderBy(x => x.Rank)
                .ToList();

        protected bool TryGetSelectedTask(out T selectedTask)
        {
            selectedTask = null;
            var children = GetChildren(CurrentParentCached);
            if (children.Count > 0 && children.Count > Cursor)
            {
                selectedTask = children[Cursor];
            }
            return selectedTask is object;
        }

        protected void EnableDefaultInsertFunctionality(string insertPrompt, Func<string, string, int, Node> nodeFactory)
        {
            CustomActions.Add(("i", (Action)(() => _io.Run(PromptToInsertNode(insertPrompt, nodeFactory)))));
            NoChildrenAction = PromptToInsertNode(insertPrompt, nodeFactory);
        }

        protected Node DefaultNodeFactory(string task, string parentId, int rank) => new TaskItem
        {
            Name = task,
            ParentID = parentId,
            Rank = rank,
        };

        protected bool TryGetSelectedItem(out T selectedTask)
        {
            selectedTask = null;
            var children = GetChildren(CurrentParentCached);
            if (children.Count > 0 && children.Count > Cursor)
            {
                selectedTask = children[Cursor];
            }
            return selectedTask is object;
        }

        private static void ConsolidateRank(List<T> tasks) => tasks.All(t => t.Rank = tasks.IndexOf(t));

        private void ChangeItemType()
        {
            if (TryGetSelectedItem(out var selectedTask))
            {
                if (ItemConversionMap.ContainsKey(selectedTask.GetType()))
                {
                    var possibleConversionsMap = ItemConversionMap[selectedTask.GetType()];
                    var actions = new List<UserAction>();
                    foreach (var keyValuePair in possibleConversionsMap)
                    {
                        var newItem = keyValuePair.Value(selectedTask);
                        void action() => ReplaceItem(selectedTask, newItem);
                        var description = $"Convert to {keyValuePair.Key.Name}";
                        var userAction = new UserAction(description, action);
                        actions.Add(userAction);
                    }
                    _io.PromptOptions($"Change the type of '{selectedTask.ToString()}'", false, actions.ToArray());
                }
            }
        }

        private void ReplaceItem(T itemToReplace, T newItem)
        {
            newItem.ID = itemToReplace.ID;
            Tree.Remove(itemToReplace);
            Tree.Set(newItem);
        }

        private void WriteHeader()
        {
            if (!string.IsNullOrWhiteSpace(_prompt))
            {
                _io.Write(_prompt);
                _io.Write();
            }

            var atParentKey = CurrentParentCached;
            var parents = new List<string>();
            while (!string.IsNullOrWhiteSpace(atParentKey) && Tree.Get(atParentKey) != null)
            {
                var atParent = Tree.Get(atParentKey);
                parents.Add(atParent.ToString());
                atParentKey = atParent.ParentID;
            }
            parents.Reverse();
            foreach (var parent in parents)
            {
                _io.WriteNoLine($"{parent} > ");
            }
            _io.Write();
        }

        private void WriteTasks(int cursorIndex, List<T> tasks, string spaces)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                var prefix = spaces.Length == 0 ? $"{i}{(i == cursorIndex ? " > " : "   ")}" : "    " + spaces;
                _io.Write($"{prefix}{tasks[i]}");
                if (ShowChildren) WriteTasks(-1, GetChildren(tasks[i].ID), spaces + "    ");
            }
        }

        private void DoTasksInteraction(string key)
        {
            SetCursor(Cursor);
            var tasks = GetChildren(CurrentParentCached);
            if (Cursor < 0 || Cursor >= tasks.Count)
            {
                return;
            }
            var task = tasks[Cursor];

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

            if (GetChildren(CurrentParentCached).Count == 0 && _goBackIfNoChild)
            {
                ViewParent();
            }
            _goBackIfNoChild = true;
            _io.Clear(_refreshDisplay);
            Edit();
        }

        private void SetCursor(int index) => Cursor = Math.Max(0, Math.Min(index, GetChildren(CurrentParentCached).Count - 1));

        private void ReparentToGrandparent(T task)
        {
            task.ParentID = Tree[task.ParentID]?.ParentID;
        }

        private void ParentUnderSibling(T task, List<T> siblings)
        {
            if (siblings.Contains(task)) siblings.Remove(task);
            if (!siblings.Any()) return;
            int i = 0;
            _io.PromptOptions($"Select the new parent for {task}", false, siblings.Select(s => new UserAction($"{i++} {s}", () => task.ParentID = s.ID)).ToArray());
        }

        private void EnterPressed(T item) => (IsParent(item) ? EnterPressedOnParentAction : EnterPressedOnLeafAction)(item);

        private bool IsParent(T item) => GetChildren(item.ID).Any();

        private void LowerTaskRank(T task)
        {
            var tasks = GetChildren(task.ParentID);
            var thisTaskIndex = tasks.IndexOf(task);
            if (thisTaskIndex > 0 && tasks.Count > 1)
            {
                var otherTask = tasks[thisTaskIndex - 1];
                otherTask.Rank++;
                task.Rank--;
                SetCursor(Cursor - 1);
            }
        }

        private void RaiseItemRank(T item)
        {
            var tasks = GetChildren(item.ParentID);
            var thisTaskIndex = tasks.IndexOf(item);
            if (thisTaskIndex < tasks.Count - 1)
            {
                var otherTask = tasks[thisTaskIndex + 1];
                otherTask.Rank--;
                item.Rank++;
                SetCursor(Cursor + 1);
            }
        }

        private void DeleteTask(T task)
        {
            Tree.Remove(task.ID);
        }

        private void CursorDown()
        {
            SetCursor(Cursor + 1);
            _refreshDisplay = false;
        }

        private void CursorUp()
        {
            SetCursor(Cursor - 1);
            _refreshDisplay = false;
        }

        private Action PromptToInsertNode(string insertPrompt, Func<string, string, int, Node> nodeFactory) => () =>
        {
            var index = NumberOfVisibleTasks;
            _io.WriteNoLine($"\n{insertPrompt}: ");

            var submitKeys = new ConsoleKey[] { ConsoleKey.Enter, ConsoleKey.LeftArrow };
            _io.Read(s => AddNode(nodeFactory, s, CurrentParentCached, index), KeyGroups.NoKeys, submitKeys);
            if (NumberOfVisibleTasks == 0) ViewParent();
        };

        private void AddNode(Func<string, string, int, Node> nodeFactory, string description, string parentID, int rank)
        {
            if (string.IsNullOrEmpty(description))
            {
                return;
            }
            var node = nodeFactory(description, parentID, rank);
            if (node is null)
            {
                return;
            }
            Tree.Set(node as T);
        }
    }
}