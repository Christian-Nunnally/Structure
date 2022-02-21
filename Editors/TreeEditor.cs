using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;
using Structure.Structure.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structure.Editors
{
    public class TreeEditor<T> where T : Node
    {
        private readonly string HELP_STRING = string.Join(Environment.NewLine, 
            "←↑↓→ - Move selection",
            "i - Insert item",
            "t - Change item type",
            "wasd - Move item in tree",
            "< Enter> - Pick item",
            "< Delete> - Delete item",
            "< Escape> - Back");
        const int NUMBER_OF_VISIBLE_ITEMS = 20;
        private readonly NodeTree<T> _tree;
        private readonly string _prompt;
        private readonly StructureIO _io;
        private readonly UserAction[] _options;
        private string _currentParentCached;
        private bool _goBackIfNoChild;
        private int _scrollIndex = 0;
        private bool _showChildren;
        private bool _return;
        private int _cursor;

        public ItemConverter<T> ItemConverter { get; set; } = new ItemConverter<T>();

        public Action<T> EnterPressedOnParentAction { get; set; }

        public Action<T> EnterPressedOnLeafAction { get; set; }

        public Action NoChildrenAction { get; set; }

        public bool ShouldExit { get; set; }

        public int NumberOfVisibleTasks => GetChildren(_currentParentCached).Count;

        public int Cursor
        {
            get => _cursor;
            set
            {
                _cursor = Math.Max(0, Math.Min(value, NumberOfVisibleTasks));
                ScrollToCursor();
            }
        }

        public TreeEditor(StructureIO io, string prompt, NodeTree<T> tree, bool allowInserting)
        {
            EnterPressedOnParentAction = SetParent;
            EnterPressedOnLeafAction = SetParent;
            NoChildrenAction = ViewParent;
            _prompt = prompt;
            _tree = tree;
            _io = io;

            _options = new UserAction[]
            {
                new UserAction("Move selection up", RunWithCurrentTask(CursorUp), ConsoleKey.UpArrow),
                new UserAction("Move selection down", RunWithCurrentTask(CursorDown), ConsoleKey.DownArrow),
                new UserAction("View parent", RunWithCurrentTask(ViewParent), ConsoleKey.LeftArrow),
                new UserAction("View child", RunWithCurrentTask(SetParent), ConsoleKey.RightArrow),
                new UserAction("Delete task", RunWithCurrentTask(DeleteItem), ConsoleKey.Delete),
                new UserAction("Complete task", RunWithCurrentTask(EnterPressed), ConsoleKey.Enter),
                new UserAction("Lower task priority", RunWithCurrentTask(LowerItemRank), ConsoleKey.W),
                new UserAction("Raise task priority", RunWithCurrentTask(RaiseItemRank), ConsoleKey.S),
                new UserAction("Reparent to grandparent", RunWithCurrentTask(ReparentToGrandparent), ConsoleKey.A),
                new UserAction("Parent under sibling", RunWithCurrentTask(ParentUnderSibling), ConsoleKey.D),
                new UserAction("Select first task", RunWithCurrentTask(CursorHome), ConsoleKey.Home),
                new UserAction("Select last task", RunWithCurrentTask(CursorEnd), ConsoleKey.End),
                new UserAction("Change item type", RunWithCurrentTask(ChangeItemType), ConsoleKey.T),
                new UserAction("Toggle show children", ToggleShowChildren, ConsoleKey.V),
                new UserAction("Exit", RunWithCurrentTask(ExitEditor), ConsoleKey.Escape),
            };
            if (allowInserting)
            {
                NoChildrenAction = PromptToInsertNode("Insert item", DefaultNodeFactory);
                _options = _options.Concat(new UserAction[]
                {
                    new UserAction("Copy current task", RunWithCurrentTask(CopyCurrentNode), ConsoleKey.C),
                    new UserAction("Insert new item", () => _io.Run(PromptToInsertNode("Insert new item", DefaultNodeFactory)), ConsoleKey.I),
                }).ToArray();
            }
        }

        public void Edit()
        {
            while (true)
            {
                var children = GetChildren(_currentParentCached);
                ConsolidateRank(children);
                WriteTasks(children, NUMBER_OF_VISIBLE_ITEMS);
                if (ShouldExit) return;
                if (children.Count == 0) _io.Run(NoChildrenAction);
                else if (!DoTasksInteraction()) break;
            }
        }

        private void WriteTasks(List<T> children, int linesToPrint)
        {
            Cursor = _cursor;
            var tasksString = new StringBuilder();
            WriteHeader(tasksString);
            WriteTasks(Cursor, children, "", ref linesToPrint, tasksString);
            _io.ClearBuffer();
            _io.Write(tasksString.ToString());
            _io.ClearStaleOutput();
        }

        private void ScrollToCursor()
        {
            if (Cursor - NUMBER_OF_VISIBLE_ITEMS >= _scrollIndex)
                _scrollIndex = Cursor - NUMBER_OF_VISIBLE_ITEMS + 1;
            else if (Cursor < _scrollIndex)
                _scrollIndex = Cursor;
        }

        public Node DefaultNodeFactory(string task, string parentId, int rank) => new TaskItem
        {
            Name = task,
            ParentID = parentId,
            Rank = rank,
        };

        private void WriteHeader(StringBuilder stringBuilder)
        {
            if (!string.IsNullOrWhiteSpace(_prompt)) stringBuilder.Append(_prompt + "\n\n");
            var atParentKey = _currentParentCached;
            var parents = new List<string>();
            while (!string.IsNullOrWhiteSpace(atParentKey) && _tree.Get(atParentKey) != null)
            {
                var atParent = _tree.Get(atParentKey);
                parents.Add(atParent.ToString());
                atParentKey = atParent.ParentID;
            }
            parents.Reverse();
            parents.All(p => stringBuilder.Append($"{p} > "));
            stringBuilder.Append("\n");
        }

        private void WriteTasks(int cursorIndex, List<T> tasks, string spaces, ref int linesToPrint, StringBuilder stringBuilder)
        {
            var canScrollUp = _scrollIndex > 0;
            var canScrollDown = tasks.Count > _scrollIndex + NUMBER_OF_VISIBLE_ITEMS;
            if (cursorIndex != -1) stringBuilder.Append(canScrollUp ? $"...\n" : "\n");
            for (int i = 0; i < tasks.Count; i++)
            {
                var prefix = spaces.Length == 0 ? $"- {(i == cursorIndex ? "> " : "  ")}" : "    " + spaces;
                if (_scrollIndex > i) continue;
                if (linesToPrint-- > 0)
                {
                    stringBuilder.Append($"{prefix}{tasks[i]}\n");
                    if (_showChildren) WriteTasks(-1, GetChildren(tasks[i].ID), spaces + "    ", ref linesToPrint, stringBuilder);
                }
            }
            var carrot = tasks.Count == cursorIndex ? "> " : "  ";
            var lastLine = spaces.Length == 0 ? $"- {carrot}" : "    " + spaces + "\n";
            var scrollDownLastLine = canScrollDown ? $"...\n" : "\n";
            if (cursorIndex != -1) stringBuilder.Append(canScrollDown ? scrollDownLastLine : lastLine);
        }

        private bool DoTasksInteraction()
        {
            _return = false;
            _io.ReadOptions("", HELP_STRING, _options);
            if (_return) return false;
            if (GetChildren(_currentParentCached).Count == 0 && _goBackIfNoChild) ViewParent();
            _goBackIfNoChild = true;
            return true;
        }

        public void SetParent(T item)
        {
            if (item == null)
            {
                _io.Run(NoChildrenAction);
                return;
            }
            _currentParentCached = item.ID;
            _goBackIfNoChild = false;
            _scrollIndex = 0;
            Cursor = 0;
        }

        public void ViewParent()
        {
            if (_currentParentCached == null) ShouldExit = true;
            var currentParent = _tree.Get(_currentParentCached);
            _currentParentCached = currentParent?.ParentID;
            Cursor = GetChildren(_currentParentCached).IndexOf(currentParent);
        }

        public List<T> GetChildren(string parent)
        {
            var childrenOfCurrentParent = _tree.Where(x => x.Value.ParentID == parent).Select(x => x.Value).OrderBy(x => x.Rank);
            return parent == null
                ? _tree.Where(x => x.Value.ParentID != null && _tree[x.Value.ParentID] == null).Select(x => x.Value).Concat(childrenOfCurrentParent).ToList()
                : childrenOfCurrentParent.ToList();
        }

        private void LowerItemRank(T item)
        {
            if (item == null) return;
            var items = GetChildren(item.ParentID);
            var thisItemIndex = items.IndexOf(item);
            if (thisItemIndex <= 0 || items.Count <= 1) return;
            var otherItem = items[thisItemIndex - 1];
            item.SwapRanks(otherItem);
            Cursor--;
        }

        private void RaiseItemRank(T item)
        {
            if (item == null) return;
            var items = GetChildren(item.ParentID);
            var thisItemIndex = items.IndexOf(item);
            if (thisItemIndex >= items.Count - 1) return;
            var otherItem = items[thisItemIndex + 1];
            item.SwapRanks(otherItem);
            Cursor++;
        }

        private Action PromptToInsertNode(string insertPrompt, Func<string, string, int, Node> nodeFactory) => () =>
        {
            var index = Cursor - 1;
            var rank = index * 2 + 1;
            _io.WriteNoLine($"\n{insertPrompt}: ");
            _io.ReadCore(s => AddNode(nodeFactory, s, _currentParentCached, rank), KeyGroups.AlphanumericKeysPlus, KeyGroups.SubmitKeys, KeyGroups.AlphanumericPlusSubmitKeys);
            if (NumberOfVisibleTasks == 0) ViewParent();
            else Cursor++;
        };

        private void AddNode(Func<string, string, int, Node> nodeFactory, string description, string parentID, int rank)
        {
            if (string.IsNullOrEmpty(description)) return;
            var node = nodeFactory(description, parentID, rank);
            if (node is null) return;
            _tree.Set(node as T);
        }

        private void DeleteItem(T item) => _tree.Remove(item?.ID);

        private void EnterPressed(T item) => (IsParent(item) ? EnterPressedOnParentAction : EnterPressedOnLeafAction)(item);

        private bool IsParent(T item) => GetChildren(item?.ID).Any();

        private void CursorDown() => Cursor++;

        private void CursorUp() => Cursor--;

        private void CursorHome() => Cursor = 0;

        private void CursorEnd() => Cursor = NumberOfVisibleTasks;

        private void ExitEditor() => _return = true;

        private void ToggleShowChildren() => _showChildren = !_showChildren;

        private void ReparentToGrandparent(T task) => task.ParentID = _tree[task.ParentID]?.ParentID;

        private static void ConsolidateRank(List<T> tasks) => tasks.All(t => t.Rank = tasks.IndexOf(t) * 2);

        private void ChangeItemType(T item)
        {
            if (item == null) return;
            if (!ItemConverter.CanConvert(item.GetType())) return;
            var posibleConversions = ItemConverter.GetPossibleConversions(_tree, item);
            _io.ReadOptions($"Change the type of '{item}'", "", posibleConversions);
        }

        private void ParentUnderSibling(T task)
        {
            var siblings = GetChildren(_currentParentCached);
            if (siblings.Contains(task)) siblings.Remove(task);
            if (!siblings.Any()) return;
            var tempTree = new NodeTree<T>();
            siblings.All(x => tempTree.Set(x.ID, x));
            var taskPicker = new ItemPicker<T>(_io, "Pick new parent", true, true, tempTree, false, x => task.ParentID = x.ID);
            _io.Run(taskPicker.Edit);
        }

        private void CopyCurrentNode(T item)
        {
            if (item == null) return;
            CopyNode(item, item.ParentID);
        }

        private void CopyNode(T node, string parentID)
        {
            var newNode = node.Copy();
            newNode.ParentID = parentID;
            _tree.Set(newNode.ID, (T)newNode);
            var children = GetChildren(node.ID);
            children.All(x => CopyNode(x, newNode.ID));
        }

        private Action RunWithCurrentTask(Action<T> interaction) => () =>
        {
            Cursor = _cursor;
            var tasks = GetChildren(_currentParentCached);
            var task = Cursor >= 0 && Cursor < tasks.Count ? tasks[Cursor] : null;
            interaction(task);
        };

        private Action RunWithCurrentTask(Action interaction) => RunWithCurrentTask(x => interaction());
    }
}