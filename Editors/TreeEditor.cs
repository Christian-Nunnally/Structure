using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;
using Structure.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structure.Editors
{
    public class TreeEditor<T> where T : Node
    {
        const int NUMBER_OF_VISIBLE_ITEMS = 20;
        private readonly ItemConverter<T> _itemConverter = new ItemConverter<T>();
        private readonly NodeTreeCollection<T> _tree;
        private readonly bool _allowInserting;
        private readonly string _prompt;
        private readonly StructureIO _io;
        private string _currentParentCached;
        private bool _goBackIfNoChild;
        private bool _refreshDisplay;
        private int _scrollIndex = 0;
        private bool _showChildren;
        private bool _return;
        private int _cursor;

        public Action<T> EnterPressedOnParentAction { get; set; }

        public Action<T> EnterPressedOnLeafAction { get; set; }

        public Action NoChildrenAction { get; set; }

        public bool ShouldExit { get; set; }

        private int Cursor
        {
            get => _cursor;
            set
            {
                var maxValue = NumberOfVisibleTasks;
                _cursor = NumberOfVisibleTasks == 1 ? 0 : Math.Max(0, Math.Min(value, maxValue));

                if (Cursor - _scrollIndex > NUMBER_OF_VISIBLE_ITEMS - 1)
                {
                    _scrollIndex++;
                    _refreshDisplay = true;
                }
                if (Cursor < _scrollIndex)
                {
                    _scrollIndex = Cursor;
                    _refreshDisplay = true;
                }
            }
        }

        public void AddToItemConversionMap(Type from, Type to, Func<T, T> conversionFunction)
        {
            _itemConverter.RegisterConversion(from, to, conversionFunction);
        }

        public TreeEditor(StructureIO io, string prompt, NodeTreeCollection<T> tree, bool allowInserting)
        {
            EnterPressedOnParentAction = SetParent;
            EnterPressedOnLeafAction = SetParent;
            NoChildrenAction = ViewParent;
            _prompt = prompt;
            _tree = tree;
            _io = io;
            _allowInserting = allowInserting;
            if (allowInserting)
            {
                NoChildrenAction = PromptToInsertNode("Insert item", DefaultNodeFactory);
            }
        }

        private void CopyCurrentNode()
        {
            if (TryGetSelectedNode(out var selectedNode))
            {
                CopyNode(selectedNode, selectedNode.ParentID);
            }
        }

        private void CopyNode(T node, string parentID)
        {
            var newNode = node.Copy();
            newNode.ParentID = parentID;
            _tree.Set(newNode.ID, (T)newNode);
            var children = GetChildren(node.ID);
            children.All(x => CopyNode(x, newNode.ID));
        }

        public int NumberOfVisibleTasks => GetChildren(_currentParentCached).Count;

        public void Edit()
        {
            while (true)
            {
                var children = GetChildren(_currentParentCached);
                ConsolidateRank(children);
                WriteHeader();
                Cursor = _cursor;
                int linesToPrint = 20;
                var tasksString = new StringBuilder();
                WriteTasks(Cursor, children, "", ref linesToPrint, tasksString);
                _io.Write(tasksString.ToString());
                if (ShouldExit) return;
                if (children.Count == 0) { NoChildrenAction(); _io.Clear(clearConsole: true); }
                else if (!DoTasksInteraction()) break;
            }
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
        }

        public void ViewParent()
        {
            if (_currentParentCached == null)
            {
                ShouldExit = true;
            }
            var currentParent = _tree.Get(_currentParentCached);
            _currentParentCached = currentParent?.ParentID;
            Cursor = GetChildren(_currentParentCached).IndexOf(currentParent);
        }

        public List<T> GetChildren(string parent)
        {
            var childrenOfCurrentParent = _tree.Where(x => x.Value.ParentID == parent)
                .Select(x => x.Value)
                .OrderBy(x => x.Rank);
            if (parent == null)
            {
                var childrenWithMissingParents = _tree.Where(x => x.Value.ParentID != null && _tree[x.Value.ParentID] == null).Select(x => x.Value);
                return childrenOfCurrentParent.Concat(childrenWithMissingParents).ToList();
            }
            return childrenOfCurrentParent.ToList();
        }

        public bool TryGetSelectedNode(out T selectedTask)
        {
            selectedTask = null;
            var children = GetChildren(_currentParentCached);
            if (children.Count > 0 && children.Count > Cursor)
            {
                selectedTask = children[Cursor];
            }
            return selectedTask is object;
        }

        public Node DefaultNodeFactory(string task, string parentId, int rank) => new TaskItem
        {
            Name = task,
            ParentID = parentId,
            Rank = rank,
        };

        protected bool TryGetSelectedItem(out T selectedTask)
        {
            selectedTask = null;
            var children = GetChildren(_currentParentCached);
            if (children.Count > 0 && children.Count > Cursor)
            {
                selectedTask = children[Cursor];
            }
            return selectedTask is object;
        }

        private static void ConsolidateRank(List<T> tasks) => tasks.All(t => t.Rank = tasks.IndexOf(t) * 2);

        private void ChangeItemType()
        {
            if (TryGetSelectedItem(out var selectedTask))
            {
                if (_itemConverter.CanConvert(selectedTask.GetType()))
                {
                    var posibleConversions = _itemConverter.GetPossibleConversions(_tree, selectedTask);
                    _io.ReadOptions($"Change the type of '{selectedTask.ToString()}'", false, "", posibleConversions);
                }
            }
        }

        private void WriteHeader()
        {
            if (!string.IsNullOrWhiteSpace(_prompt))
            {
                _io.Write(_prompt);
                _io.Write();
            }

            var atParentKey = _currentParentCached;
            var parents = new List<string>();
            while (!string.IsNullOrWhiteSpace(atParentKey) && _tree.Get(atParentKey) != null)
            {
                var atParent = _tree.Get(atParentKey);
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

        private void WriteTasks(int cursorIndex, List<T> tasks, string spaces, ref int linesToPrint, StringBuilder stringBuilder)
        {
            var canScrollUp = _scrollIndex > 0;
            var canScrollDown = tasks.Count > _scrollIndex + NUMBER_OF_VISIBLE_ITEMS;
            stringBuilder.Append(canScrollUp ? $"...\n" : "\n");
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
            stringBuilder.Append(canScrollDown ? scrollDownLastLine : lastLine);
        }

        private bool DoTasksInteraction()
        {
            _return = false;
            var options = new List<UserAction>
            {
                new UserAction("Move selection up", EditorInteractionWrapper(CursorUp), ConsoleKey.UpArrow),
                new UserAction("Move selection down", EditorInteractionWrapper(CursorDown), ConsoleKey.DownArrow),
                new UserAction("View parent", EditorInteractionWrapper(ViewParent), ConsoleKey.LeftArrow),
                new UserAction("View child", EditorInteractionWrapper(SetParent), ConsoleKey.RightArrow),
                new UserAction("Delete task", EditorInteractionWrapper(DeleteItem), ConsoleKey.Delete),
                new UserAction("Complete task", EditorInteractionWrapper(EnterPressed), ConsoleKey.Enter),
                new UserAction("Lower task priority", EditorInteractionWrapper(LowerItemRank), ConsoleKey.W),
                new UserAction("Raise task priority", EditorInteractionWrapper(RaiseItemRank), ConsoleKey.S),
                new UserAction("Reparent to grandparent", EditorInteractionWrapper(ReparentToGrandparent), ConsoleKey.A),
                new UserAction("Parent under sibling", EditorInteractionWrapper(ParentUnderSibling), ConsoleKey.D),
                new UserAction("Select first task", EditorInteractionWrapper(CursorHome), ConsoleKey.Home),
                new UserAction("Select last task", EditorInteractionWrapper(CursorEnd), ConsoleKey.End),
                new UserAction("Change item type", ChangeItemType, ConsoleKey.T),
                new UserAction("Toggle show children", ToggleShowChildren, ConsoleKey.V),
                new UserAction("Exit", EditorInteractionWrapper(ExitEditor), ConsoleKey.Escape),
            };
            if (_allowInserting)
            {
                options.Add(new UserAction("Copy current task", CopyCurrentNode, ConsoleKey.C));
                options.Add(new UserAction("Insert new item", () => _io.Run(PromptToInsertNode("Insert new item", DefaultNodeFactory)), ConsoleKey.I));
            }

            var helpString = "←↑↓→ - Move selection\n";
            helpString += "i - Insert item\n";
            helpString += "t - Change item type\n";
            helpString += "wasd - Move item in tree\n";
            helpString += "<Enter> - Pick item\n";
            helpString += "<Delete> - Delete item\n";
            helpString += "<Escape> - Back\n";

            _io.ReadOptions("", false, helpString, options.ToArray());
            if (_return) return false;
            if (GetChildren(_currentParentCached).Count == 0 && _goBackIfNoChild) ViewParent();
            _goBackIfNoChild = true;
            _io.Clear(_refreshDisplay);
            return true;
        }

        private void ToggleShowChildren() => _showChildren = !_showChildren;

        private Action EditorInteractionWrapper(Action<T> interaction)
        {
            return () =>
            {
                Cursor = _cursor;
                var tasks = GetChildren(_currentParentCached);
                if (Cursor < 0 || Cursor > tasks.Count)
                {
                    _return = true;
                    return;
                }
                var task = Cursor != tasks.Count ? tasks[Cursor] : null;
                _refreshDisplay = true;
                interaction(task);
            };
        }

        private Action EditorInteractionWrapper(Action interaction) => EditorInteractionWrapper(x => interaction());

        private void ReparentToGrandparent(T task) => task.ParentID = _tree[task.ParentID]?.ParentID;

        private void ParentUnderSibling(T task)
        {
            var siblings = GetChildren(_currentParentCached);
            if (siblings.Contains(task)) siblings.Remove(task);
            if (!siblings.Any()) return;
            var tempTree = new NodeTreeCollection<T>();
            siblings.All(x => tempTree.Set(x.ID, x));
            var taskPicker = new ItemPicker<T>(_io, "Pick new parent", true, true, tempTree, false, x => task.ParentID = x.ID);
            _io.Run(taskPicker.Edit);
        }

        private void EnterPressed(T item)
        { 
            if (item != null) (IsParent(item) ? EnterPressedOnParentAction : EnterPressedOnLeafAction)(item); 
        }

        private bool IsParent(T item) => GetChildren(item.ID).Any();

        private void LowerItemRank(T item)
        {
            if (item == null) return;
            var items = GetChildren(item.ParentID);
            var thisItemIndex = items.IndexOf(item);
            if (thisItemIndex > 0 && items.Count > 1)
            {
                var otherItem = items[thisItemIndex - 1];
                otherItem.Rank++;
                item.Rank--;
                Cursor--;
            }
        }

        private void RaiseItemRank(T item)
        {
            if (item == null) return;
            var items = GetChildren(item.ParentID);
            var thisItemIndex = items.IndexOf(item);
            if (thisItemIndex < items.Count - 1)
            {
                var otherTask = items[thisItemIndex + 1];
                otherTask.Rank--;
                item.Rank++;
                Cursor++;
            }
        }

        private void DeleteItem(T item)
        {
            if (item == null) return;
            _tree.Remove(item.ID);
        }

        private void CursorDown()
        {
            _refreshDisplay = false;
            Cursor++;
        }

        private void CursorUp()
        {
            _refreshDisplay = false;
            Cursor--;
        }

        private void CursorHome()
        {
            _refreshDisplay = false;
            Cursor = 0;
        }

        private void CursorEnd()
        {
            _refreshDisplay = false;
            Cursor = NumberOfVisibleTasks;
        }

        private void ExitEditor() => _return = true;

        private Action PromptToInsertNode(string insertPrompt, Func<string, string, int, Node> nodeFactory) => () =>
        {
            var index = Cursor - 1;
            _io.WriteNoLine($"\n{insertPrompt}: ");

            var submitKeys = new ConsoleKey[] { ConsoleKey.Enter, ConsoleKey.LeftArrow };
            _io.Read(s => AddNode(nodeFactory, s, _currentParentCached, index * 2 + 1), KeyGroups.AlphanumericKeysPlus, submitKeys);
            if (NumberOfVisibleTasks == 0) ViewParent();
            Cursor++;
        };

        private void AddNode(Func<string, string, int, Node> nodeFactory, string description, string parentID, int rank)
        {
            if (string.IsNullOrEmpty(description)) return;
            var node = nodeFactory(description, parentID, rank);
            if (node is null) return;
            _tree.Set(node as T);
        }
    }
}