using Structur.IO;
using Structur.IO.Persistence;
using Structur.TaskItems;
using System;

namespace Structur.Editors.Obsolete
{
    public class TaskPickerObsolete : TreeEditorObsolete<TaskItem>
    {
        private Action<TaskItem> _pickedAction;
        private readonly string _pickPrompt;
        private readonly bool _exitAfterPick;
        private readonly StructureIO _io;

        public TaskPickerObsolete(
            StructureIO io,
            string prompt,
            string pickPrompt,
            bool allowLeafs,
            bool allowParents,
            bool exitAfterPick,
            NodeTree<TaskItem> tree,
            Action<TaskItem> pickedAction = null)
            : base(io, prompt, tree)
        {
            if (allowLeafs) EnterPressedOnLeafAction = ConfirmPick;
            if (allowParents) EnterPressedOnParentAction = ConfirmPick;

            _pickPrompt = pickPrompt;
            _exitAfterPick = exitAfterPick;
            _pickedAction = pickedAction;
            _io = io;
        }

        public void SetPickAction(Action<TaskItem> action)
        {
            _pickedAction = action;
        }

        private void ConfirmPick(TaskItem task) => _io.ReadOptions($"{_pickPrompt} {task}?", "",
            new UserAction("no", () => { }, ConsoleKey.N),
            new UserAction("yes (Enter)", () => Pick(task), ConsoleKey.Enter),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.Y),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.W),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.DownArrow),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.D5),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.D2),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.D1),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.D0),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.Oem7),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.RightArrow),
            new UserAction("yes (Y)", () => Pick(task), ConsoleKey.LeftArrow));

        private void Pick(TaskItem task)
        {
            _pickedAction(task);
            if (_exitAfterPick) ShouldExit = true;
        }
    }
}