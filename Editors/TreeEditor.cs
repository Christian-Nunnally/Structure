using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;
using Structure.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Editors
{
    public class TreeEditor<T> where T : Node
    {
        private int _scrollIndex = 0;

        public List<UserAction> CustomActions { get; } = new List<UserAction>();

        public Action<T> EnterPressedOnParentAction { get; set; }

        public Action<T> EnterPressedOnLeafAction { get; set; }

        public Action NoChildrenAction { get; set; }

        protected Dictionary<Type, Dictionary<Type, Func<T, T>>> ItemConversionMap { get; } = new Dictionary<Type, Dictionary<Type, Func<T, T>>>();

        protected string CurrentParentCached { get; set; }

        protected NodeTreeCollection<T> Tree { get; set; }

        public bool ShowChildren { get; set; }

        public bool ShouldExit { get; set; }

        protected int Cursor
        {
            get => _cursor;
            set
            {
                var maxValue = NumberOfVisibleTasks;
                _cursor = NumberOfVisibleTasks == 1 ? 0 : Math.Max(0, Math.Min(value, maxValue));
            }
        }

        private readonly string _prompt;
        private bool _refreshDisplay;
        private bool _goBackIfNoChild;
        private bool _return;
        private int _cursor;
        private readonly StructureIO _io;

        public void AddToItemConversionMap(Type from, Type to, Func<T, T> conversionFunction)
        {
            if (!ItemConversionMap.ContainsKey(from)) ItemConversionMap.Add(from, new Dictionary<Type, Func<T, T>>());
            var dictionary = ItemConversionMap[from];
            if (!dictionary.ContainsKey(to)) dictionary.Add(to, conversionFunction);
        }

        public TreeEditor(StructureIO io, string prompt, NodeTreeCollection<T> tree)
        {
            EnterPressedOnParentAction = SetParent;
            EnterPressedOnLeafAction = SetParent;
            NoChildrenAction = ViewParent;
            CustomActions.Add(new UserAction("Change item type", ChangeItemType, ConsoleKey.T));
            _prompt = prompt;
            Tree = tree;
            _io = io;
        }

        public T CurrentParent => CurrentParentCached == null ? null : Tree.Get(CurrentParentCached);

        public int NumberOfVisibleTasks => GetChildren(CurrentParentCached).Count;

        public void Edit()
        {
            while (true)
            {
                var children = GetChildren(CurrentParentCached);
                ConsolidateRank(children);
                WriteHeader();
                Cursor = _cursor;
                if (Cursor - _scrollIndex > 19)
                {
                    _scrollIndex++;
                }
                if (Cursor < _scrollIndex)
                {
                    _scrollIndex = Cursor;
                }
                int linesToPrint = 20;
                WriteTasks(Cursor, children, "", ref linesToPrint);
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
            Cursor = GetChildren(CurrentParentCached).IndexOf(currentParent);
        }

        public List<T> GetChildren(string parent)
        {
            var childrenOfCurrentParent = Tree.Where(x => x.Value.ParentID == parent)
                .Select(x => x.Value)
                .OrderBy(x => x.Rank);
            if (parent == null)
            {
                var childrenWithMissingParents = Tree.Where(x => x.Value.ParentID != null && Tree[x.Value.ParentID] == null).Select(x => x.Value);
                return childrenOfCurrentParent.Concat(childrenWithMissingParents).ToList();
            }
            return childrenOfCurrentParent.ToList();
        }

        public bool TryGetSelectedTask(out T selectedTask)
        {
            selectedTask = null;
            var children = GetChildren(CurrentParentCached);
            if (children.Count > 0 && children.Count > Cursor)
            {
                selectedTask = children[Cursor];
            }
            return selectedTask is object;
        }

        public void EnableDefaultInsertFunctionality(string insertPrompt, Func<string, string, int, Node> nodeFactory)
        {
            CustomActions.Add(new UserAction("Insert new item", () => _io.Run(PromptToInsertNode(insertPrompt, nodeFactory)), ConsoleKey.I));
            NoChildrenAction = PromptToInsertNode(insertPrompt, nodeFactory);
        }

        public static Node DefaultNodeFactory(string task, string parentId, int rank) => new TaskItem
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

        private static void ConsolidateRank(List<T> tasks) => tasks.All(t => t.Rank = tasks.IndexOf(t) * 2);

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
                    _io.PromptOptions($"Change the type of '{selectedTask.ToString()}'", false, "", actions.ToArray());
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

        private void WriteTasks(int cursorIndex, List<T> tasks, string spaces, ref int linesToPrint)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                var prefix = spaces.Length == 0 ? $"- {(i == cursorIndex ? "> " : "  ")}" : "    " + spaces;

                if (_scrollIndex > i) continue;
                if (linesToPrint-- > 0)
                {
                    _io.Write($"{prefix}{tasks[i]}");
                    if (ShowChildren) WriteTasks(-1, GetChildren(tasks[i].ID), spaces + "    ", ref linesToPrint);
                }
                else
                {
                    if (cursorIndex == i) _scrollIndex++;
                }
            }
            var lastLine = spaces.Length == 0 ? $"- {(tasks.Count == cursorIndex ? "> " : "  ")}" : "    " + spaces;
            _io.Write(lastLine);
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
                new UserAction("Exit", EditorInteractionWrapper(ExitEditor), ConsoleKey.Escape),
            };
            CustomActions.All(x => options.Add(new UserAction(x.Description, EditorInteractionWrapper(x.Action), x.Hotkey.Key)));

            var helpString = "←↑↓→ - Move selection\n";
            helpString += "i - Insert item\n";
            helpString += "t - Change item type\n";
            helpString += "wasd - Move item in tree\n";
            helpString += "<Enter> - Pick item\n";
            helpString += "<Delete> - Delete item\n";
            helpString += "<Escape> - Back\n";

            _io.PromptOptions("", false, helpString, options.ToArray());
            if (_return) return false;
            if (GetChildren(CurrentParentCached).Count == 0 && _goBackIfNoChild)
            {
                ViewParent();
            }
            _goBackIfNoChild = true;
            _io.Clear(_refreshDisplay);

            return true;
        }

        private Action EditorInteractionWrapper(Action<T> interaction)
        {
            return () =>
            {
                Cursor = _cursor;
                var tasks = GetChildren(CurrentParentCached);
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

        private void ReparentToGrandparent(T task) => task.ParentID = Tree[task.ParentID]?.ParentID;

        private void ParentUnderSibling(T task)
        {
            var siblings = GetChildren(CurrentParentCached);
            if (siblings.Contains(task)) siblings.Remove(task);
            if (!siblings.Any()) return;
            int i = 0;
            var tempTree = new NodeTreeCollection<T>();
            siblings.All(x => tempTree.Set(x.ID, x));
            var taskPicker = new ItemPicker<T>(_io, "Pick new parent", true, true, true, tempTree, x => task.ParentID = x.ID);
            _io.Run(taskPicker.Edit);
        }

        private void EnterPressed(T item)
        { 
            if (item != null) 
            { 
                (IsParent(item) ? EnterPressedOnParentAction : EnterPressedOnLeafAction)(item); 
            }
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
            Tree.Remove(item.ID);
        }

        private void CursorDown()
        {
            Cursor++;
            _refreshDisplay = false;
        }

        private void CursorUp()
        {
            Cursor--;
            _refreshDisplay = false;
        }

        private void CursorHome()
        {
            Cursor = 0;
            _refreshDisplay = false;
        }

        private void CursorEnd()
        {
            Cursor = NumberOfVisibleTasks;
            _refreshDisplay = false;
        }

        private void ExitEditor() => _return = true;

        private Action PromptToInsertNode(string insertPrompt, Func<string, string, int, Node> nodeFactory) => () =>
        {
            var index = Cursor - 1;
            _io.WriteNoLine($"\n{insertPrompt}: ");

            var submitKeys = new ConsoleKey[] { ConsoleKey.Enter, ConsoleKey.LeftArrow };
            _io.Read(s => AddNode(nodeFactory, s, CurrentParentCached, index * 2 + 1), KeyGroups.AlphanumericKeysPlus, submitKeys);
            if (NumberOfVisibleTasks == 0) ViewParent();
            Cursor++;
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