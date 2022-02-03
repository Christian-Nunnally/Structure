using System;

namespace Structure
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
            NodeTreeCollection<TaskItem> tree,
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

        private void ConfirmPick(TaskItem task) => _io.PromptOptions($"{_pickPrompt} {task}?", true, "",
            new UserAction("no", () => { }),
            new UserAction("yes (Enter)", () =>
            {
                _pickedAction(task);
                if (_exitAfterPick) ShouldExit = true;
            }));
    }
}