using System;

namespace Structure
{
    public class TaskPicker : TreeEditor<TaskItem>
    {
        protected Action<TaskItem> PickedAction;
        private readonly string _pickPrompt;
        private readonly bool _exitAfterPick;
        private readonly StructureIO _io;

        public TaskPicker(
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
            PickedAction = pickedAction;
            _io = io;
        }

        private void ConfirmPick(TaskItem task) => _io.PromptOptions($"{_pickPrompt} {task}?", true,
            new UserAction("no", () => { }),
            new UserAction("yes (Enter)", () =>
            {
                PickedAction(task);
                if (_exitAfterPick) ShouldExit = true;
            }));
    }
}