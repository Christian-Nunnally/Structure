using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure
{
    public class TreeEditor<ItemType> where ItemType : Node
    {
        public readonly List<(string, Action)> CustomActions = new List<(string, Action)>();
        public Action<ItemType> EnterPressedOnParentAction;
        public Action<ItemType> EnterPressedOnLeafAction;
        public Action NoChildrenAction;
        protected Dictionary<Type, Dictionary<Type, Func<ItemType, ItemType>>> ItemConversionMap = new Dictionary<Type, Dictionary<Type, Func<ItemType, ItemType>>>();
        protected string _currentParent;
        protected bool EnableReparenting = true;
        protected NodeTreeCollection<ItemType> Tree;
        protected bool ShowChildren;
        protected bool ShouldExit;
        protected int _cursor = 0;
        private readonly string _prompt;
        private bool _refreshDisplay;
        private bool _goBackIfNoChild;
        private readonly StructureIO _io;

        public TreeEditor(StructureIO io, string prompt, NodeTreeCollection<ItemType> tree)
        {
            EnterPressedOnParentAction = SetParent;
            EnterPressedOnLeafAction = SetParent;
            NoChildrenAction = () => { _io.News("No children"); ViewParent(); };
            CustomActions.Add(("t", ChangeItemType));
            _prompt = prompt;
            Tree = tree;
            _io = io;
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
            if (children.Count == 0) { NoChildrenAction(); _io.Clear(); Edit(); }
            else _io.ReadKey(x => DoTasksInteraction(x));
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

        protected bool TryGetSelectedTask(out ItemType selectedTask)
        {
            selectedTask = null;
            var children = GetChildren(_currentParent);
            if (children.Count > 0 && children.Count > _cursor)
            {
                selectedTask = children[_cursor];
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

        protected bool TryGetSelectedItem(out ItemType selectedTask)
        {
            selectedTask = null;
            var children = GetChildren(_currentParent);
            if (children.Count > 0 && children.Count > _cursor)
            {
                selectedTask = children[_cursor];
            }
            return selectedTask is object;
        }

        private static void ConsolidateRank(List<ItemType> tasks) => tasks.All(t => t.Rank = tasks.IndexOf(t));

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

        private void ReplaceItem(ItemType itemToReplace, ItemType newItem)
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

            var atParentKey = _currentParent;
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

        private void WriteTasks(int cursorIndex, List<ItemType> tasks, string spaces)
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
            SetCursor(_cursor);
            var tasks = GetChildren(_currentParent);
            if (_cursor < 0 || _cursor >= tasks.Count)
            {
                return;
            }
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
            _io.Clear(_refreshDisplay);
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
            _io.PromptOptions($"Select the new parent for {task}", false, siblings.Select(s => new UserAction($"{i++} {s}", () => task.ParentID = s.ID)).ToArray());
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
            _io.WriteNoLine($"\n{insertPrompt}: ");
            _io.Read(s => AddNode(nodeFactory, s, _currentParent, index), ConsoleKey.Enter, ConsoleKey.LeftArrow);
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
            Tree.Set(node as ItemType);
        }
    }
}