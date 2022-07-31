using Structur.IO;
using Structur.IO.Persistence;
using Structur.TaskItems;
using Structur.Program.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.ObjectModel;

namespace Structur.Editors.Obsolete
{
    public class TreeEditorObsolete<T> where T : Node
    {
        public IList<UserAction> CustomActions { get; } = new List<UserAction>();
        public Action<T> EnterPressedOnParentAction { get; set; }
        public Action<T> EnterPressedOnLeafAction { get; set; }
        public Action NoChildrenAction { get; set; }
        protected ItemConverter<T> ItemConverter { get; set;  } = new ItemConverter<T>();
        protected string CurrentParentCached { get; set; }
        protected bool EnableReparenting { get; set; } = true;
        protected NodeTree<T> Tree { get; set; }
        protected bool ShowChildren { get; set; }
        public bool ShouldExit { get; set; }
        protected int Cursor { get; set; }

        private readonly string _prompt;
        private bool _goBackIfNoChild;
        private bool _return;
        private readonly StructureIO _io;
        private readonly List<UserAction> _options;

        public TreeEditorObsolete(StructureIO io, string prompt, NodeTree<T> tree)
        {
            EnterPressedOnParentAction = SetParent;
            EnterPressedOnLeafAction = SetParent;
            NoChildrenAction = ViewParent;
            CustomActions.Add(new UserAction("t", ChangeItemType));
            _prompt = prompt;
            Tree = tree;
            _io = io;
            _options = new List<UserAction>
            {
                new UserAction("Move selection up", EditorInteractionWrapper(CursorUp), ConsoleKey.UpArrow),
                new UserAction("Move selection down", EditorInteractionWrapper(CursorDown), ConsoleKey.DownArrow),
                new UserAction("View parent", EditorInteractionWrapper(ViewParent), ConsoleKey.LeftArrow),
                new UserAction("View child", EditorInteractionWrapper(SetParent), ConsoleKey.RightArrow),
                new UserAction("Delete task", EditorInteractionWrapper(DeleteTask), ConsoleKey.Delete),
                new UserAction("Complete task", EditorInteractionWrapper(EnterPressed), ConsoleKey.Enter),
                new UserAction("Lower task priority", EditorInteractionWrapper(LowerTaskRank), ConsoleKey.W),
                new UserAction("Raise task priority", EditorInteractionWrapper(RaiseItemRank), ConsoleKey.S),
            };
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
                SetCursor(Cursor);
                WriteTasks(Cursor, children, "");
                _io.ClearStaleOutput();
                if (ShouldExit) return;
                if (children.Count == 0) _io.Run(NoChildrenAction);
                else if (!DoTasksInteraction()) break;
            }
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

        public IList<T> GetChildren(string parent) =>
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
            CustomActions.Add(new UserAction("i", () => _io.Run(PromptToInsertNode(insertPrompt, nodeFactory)), ConsoleKey.I));
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

        private static void ConsolidateRank(IList<T> tasks) => tasks.All(t => t.Rank = tasks.IndexOf(t));

        private void ChangeItemType()
        {
            if (TryGetSelectedItem(out var item))
            {
                if (item == null) return;
                if (!ItemConverter.CanConvert(item.GetType())) return;
                var posibleConversions = ItemConverter.GetPossibleConversions(Tree, item);
                _io.ReadOptions($"Change the type of '{item}'", "", posibleConversions);
            }
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

        private void WriteTasks(int cursorIndex, IList<T> tasks, string spaces)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                var prefix = spaces.Length == 0 ? $"-{(i == cursorIndex ? " > " : "   ")}" : "    " + spaces;
                _io.Write($"{prefix}{tasks[i]}");
                if (ShowChildren) WriteTasks(-1, GetChildren(tasks[i].ID), spaces + "    ");
            }
        }

        private bool DoTasksInteraction()
        {
            _return = false;
            var options = _options.ToList();
            if (EnableReparenting)
            {
                options.Add(new UserAction("Reparent to grandparent", EditorInteractionWrapper(ReparentToGrandparent), ConsoleKey.A));
                options.Add(new UserAction("Parent under sibling", EditorInteractionWrapper(ParentUnderSibling), ConsoleKey.D));
            }
            for (var i = 0; i < 9; i++)
            {
                var b = i;
                options.Add(new UserAction($"{i}", EditorInteractionWrapper(() => SetCursor(b))));
            }
            CustomActions.All(x => options.Add(!x.HotkeyOverridden ? new UserAction(x.Description, EditorInteractionWrapper(x.Action)) : new UserAction(x.Description, EditorInteractionWrapper(x.Action), x.Hotkey.Key)));

            options.Add(new UserAction("exit", EditorInteractionWrapper(() => { _return = true; }), ConsoleKey.Escape));

            _io.ReadOptionsObsolete("", "", options.ToArray());
            if (_return) return false;
            if (GetChildren(CurrentParentCached).Count == 0 && _goBackIfNoChild)
            {
                ViewParent();
            }
            _goBackIfNoChild = true;
            _io.ClearBuffer();

            return true;
        }

        private Action EditorInteractionWrapper(Action<T> interaction)
        {
            return () =>
            {
                SetCursor(Cursor);
                var tasks = GetChildren(CurrentParentCached);
                if (Cursor < 0 || Cursor >= tasks.Count)
                {
                    _return = true;
                    return;
                }
                var task = tasks[Cursor];
                interaction(task);
            };
        }

        private Action EditorInteractionWrapper(Action interaction) => EditorInteractionWrapper(x => interaction());

        private void SetCursor(int index) => Cursor = Math.Max(0, Math.Min(index, GetChildren(CurrentParentCached).Count - 1));

        private void ReparentToGrandparent(T task) => task.ParentID = Tree[task.ParentID]?.ParentID;

        private void ParentUnderSibling(T task)
        {
            var siblings = GetChildren(CurrentParentCached);
            if (siblings.Contains(task)) siblings.Remove(task);
            if (!siblings.Any()) return;
            int i = 0;
            var options = siblings.Select(s => new UserAction($"{i++} {s}", () => task.ParentID = s.ID)).ToList();
            _io.ReadOptionsObsolete($"Select the new parent for {task}", "", options.ToArray());
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
        }

        private void CursorUp()
        {
            SetCursor(Cursor - 1);
        }

        private Action PromptToInsertNode(string insertPrompt, Func<string, string, int, Node> nodeFactory) => () =>
        {
            var index = NumberOfVisibleTasks;
            _io.WriteNoLine($"\n{insertPrompt}: ");

            _io.ReadCore(s => AddNode(nodeFactory, s, CurrentParentCached, index), KeyGroups.AlphanumericInputKeys, KeyGroups.SubmitKeys, KeyGroups.AlphanumericPlusSubmitKeys);
            if (NumberOfVisibleTasks == 0) ViewParent();
        };

        private void AddNode(Func<string, string, int, Node> nodeFactory, string description, string parentID, int rank)
        {
            if (string.IsNullOrEmpty(description)) return;
            var node = nodeFactory(description, parentID, rank);
            if (node is null) return;
            Tree.Set(node as T);
        }
    }
}